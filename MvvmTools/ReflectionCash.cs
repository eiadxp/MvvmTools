using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MvvmTools
{
    static class ReflectionCash
    {
        static TValue GetValue<TKey,TValue>(this Dictionary<TKey,TValue> dictionary, TKey key, Func<TValue> getDefaultValue)
        {
            if (!dictionary.TryGetValue(key, out TValue value))
            {
                value = getDefaultValue();
                dictionary.Add(key, value);
            }
            return value;
        }
        public static Type GetType(string name)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var type = assembly.GetExportedTypes().FirstOrDefault(t => t.Name == name);
                if (type != null) return type;
            }
            return null;
        }
        #region Events
        static Dictionary<Type, EventInfo[]> _Events = new Dictionary<Type, EventInfo[]>();
        static Dictionary<Type, EventInfo> _DefaultEvent = new Dictionary<Type, EventInfo>();
        public static EventInfo[] GetEvents(Type eventSource) => _Events.GetValue(eventSource, () => eventSource.GetEvents());
        public static EventInfo GetEvent(Type eventSource, string eventName) => GetEvents(eventSource).SingleOrDefault(e => e.Name == eventName);
        public static EventInfo GetDefaultEvent(Type eventSource)
        {
            var e = _DefaultEvent.GetValue(eventSource, () =>
            {
                var a = eventSource.GetCustomAttribute<System.ComponentModel.DefaultEventAttribute>();
                if (a == null) return ReflectionCash.GetEvents(eventSource).FirstOrDefault();
                return ReflectionCash.GetEvents(eventSource).FirstOrDefault(ee => ee.Name == a.Name);
            });
            if (e == null) e = GetEvents(eventSource).FirstOrDefault();
            return e;
        }
        public static EventInfo GetEventOrDefault(Type eventSource, string eventName)
        {
            return string.IsNullOrWhiteSpace(eventName)
                ? GetDefaultEvent(eventSource)
                : GetEvents(eventSource).SingleOrDefault(e => e.Name == eventName);
        }
        #endregion
        #region Handlers
        static Dictionary<Type, ParameterInfo[]> _HandlerParameters = new Dictionary<Type, ParameterInfo[]>();
        public static Type GetHandlerType(Type eventSource, string eventName) => GetEvent(eventSource, eventName)?.EventHandlerType;
        public static ParameterInfo[] GetHandlerParameter(Type handlerType) => _HandlerParameters.GetValue(handlerType, () => handlerType.GetMethod("Invoke").GetParameters());
        #endregion
        #region Methods
        static Dictionary<Type, MethodInfo[]> _Methods = new Dictionary<Type, MethodInfo[]>();
        static Dictionary<MethodInfo, ParameterInfo[]> _MethodParameters = new Dictionary<MethodInfo, ParameterInfo[]>();
        public static MethodInfo[] GetMethods(Type type) => _Methods.GetValue(type, () => type.GetMethods());
        public static ParameterInfo[] GetMethodParameters(MethodInfo method) => _MethodParameters.GetValue(method, () => method.GetParameters());
        public static void ExecuteMethod(object methodSource, string methodName)
        {
            methodSource = methodSource?? throw new NullReferenceException("Method source can not be null.");
            var action = ReflectionCash.GetDelegateWithoutParameter(methodSource.GetType(), methodName) ??
                throw new InvalidOperationException($"Can not find method {methodName} in object of type {methodSource.GetType()}.");
            action(methodSource);
        }
        public static void ExecuteMethod(object methodSource, string methodName, object parameter, Type parameterType = null)
        {
            methodSource = methodSource?? throw new NullReferenceException("Method source can not be null.");
            parameterType = parameterType ?? parameter?.GetType() ?? typeof(object);
            var action = ReflectionCash.GetDelegateWithParameter(methodSource.GetType(), methodName, parameterType) ??
                throw new InvalidOperationException($"Can not find method {methodName} in object of type {methodSource.GetType()}.");
            action(methodSource, parameter);
        }
        #endregion
        #region Parameterless Method Delegate
        static Dictionary<MethodInfo, Action<object>> _DelegatesOfMethodsWithoutParameter = new Dictionary<MethodInfo, Action<object>>();
        public static MethodInfo GetMethodInfoWithoutParameter(Type type, string method) 
            => GetMethods(type).SingleOrDefault(m => m.Name.Equals(method, StringComparison.Ordinal) && GetMethodParameters(m).Length == 0);
        public static Action<object> GetDelegateWithoutParameter(MethodInfo method)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));
            return _DelegatesOfMethodsWithoutParameter.GetValue(method, () => CreateDelegateWithoutParameter(method));
        }
        public static Action<object> GetDelegateWithoutParameter(Type type, string method) => GetDelegateWithoutParameter(GetMethodInfoWithoutParameter(type, method));
        static Action<object> CreateDelegateWithoutParameter(MethodInfo method)
        {
            // Parameterless function should be like this:
            // void (object obj) => ((TYPE)obj).METHOD();
            // Where TYPE is the declaring type of the method.
            var delegateType = typeof(Action<object>);
            var delegateParameter = Expression.Parameter(typeof(object));
            var convertedParameter = Expression.Convert(delegateParameter, method.DeclaringType);
            var methodCall = Expression.Call(convertedParameter, method);
            return Expression.Lambda<Action<object>>(methodCall, new[] { delegateParameter }).Compile();
        }
        #endregion
        #region Method With Parameter Delegate
        static Dictionary<MethodInfo, Action<object, object>> _DelegatesOfMethodsWithParameter = new Dictionary<MethodInfo, Action<object, object>>();
        static MethodInfo GetMethodInfoWithParameter(Type type, string method, Type parameterType)
        {
            parameterType = parameterType ?? typeof(object);
            var methods = GetMethods(type).Where(m => m.Name.Equals(method, StringComparison.Ordinal) && GetMethodParameters(m).Length == 1).ToList();
            var item = methods.SingleOrDefault(m => GetMethodParameters(m)[0].ParameterType.IsAssignableFrom(parameterType));
            item = item ?? methods.SingleOrDefault(m => GetMethodParameters(m)[0].ParameterType == typeof(object));
            item = item ?? methods.FirstOrDefault();
            if (item == null) throw new InvalidOperationException($"Can not find method {method} with single parameter.");
            return item;
        }
        public static Action<object, object> GetDelegateWithParameter(MethodInfo method)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));
            return _DelegatesOfMethodsWithParameter.GetValue(method, () => CreateDelegateWithParameter(method));
        }
        public static Action<object, object> GetDelegateWithParameter(Type type, string method, Type parameterType)
            => GetDelegateWithParameter(GetMethodInfoWithParameter(type, method, parameterType));
        public static Action<object, object> CreateDelegateWithParameter(MethodInfo method)
        {
            var parameters = GetMethodParameters(method);
            if (parameters.Length == 0) throw new InvalidOperationException($"Method {method.Name} has no parameters.");
            if (parameters.Length > 1) throw new InvalidOperationException($"Method {method.Name} has more than one parameters.");
            // Parametered function should be like this:
            // void (object obj, object parameter) => ((TYPE)obj).METHOD((PARAMETERTYPE)parameter);
            // Where TYPE is the declaring type of the method.
            // and PARAMETERTYPE is the parameter type from the method info.
            var delegateType = typeof(Action<object, object>);
            var delegateObj = Expression.Parameter(typeof(object));
            var convertedObj = Expression.Convert(delegateObj, method.DeclaringType);
            var delegateParameter = Expression.Parameter(typeof(object));
            var convertedParameter = Expression.Convert(delegateParameter, parameters[0].ParameterType);
            var methodCall = Expression.Call(convertedObj, method, new[] { convertedParameter });
            return Expression.Lambda<Action<object, object>>(methodCall, new[] { delegateObj, delegateParameter }).Compile();
        }
        #endregion
        #region Properties
        static Dictionary<Type, PropertyInfo[]> _Properties = new Dictionary<Type, PropertyInfo[]>();
        public static object GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            var type = obj.GetType();
            var property = _Properties.GetValue(type, () => type.GetProperties()).SingleOrDefault(p => p.Name == propertyName);
            if (property == null) throw new InvalidOperationException($"Can not find property '{propertyName}' in type '{type.Name}'.");
            return property.GetValue(obj);
        }
        #endregion
    }
}
