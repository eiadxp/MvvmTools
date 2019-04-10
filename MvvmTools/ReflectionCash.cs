using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Input;

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
        public static void ExecuteCommandOrMethod(object source, string name)
        {
            source = source ?? throw new NullReferenceException("Method source can not be null.");
            if (!ExecuteCommand(source, name)) ExecuteMethod(source, name);
        }
        public static void ExecuteCommandOrMethod(object source, string name, object parameter, Type parameterType = null)
        {
            source = source ?? throw new NullReferenceException("Method source can not be null.");
            if (!ExecuteCommand(source, name, parameter)) ExecuteMethod(source, name, parameter, parameterType);
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
                var a = eventSource.GetCustomAttribute<System.ComponentModel.DefaultEventAttribute>()
                        ?? throw new InvalidOperationException($"Type {eventSource.Name} does not own a default event.");
                return ReflectionCash.GetEvent(eventSource, a.Name);
            });
            if (e == null) e = GetEvents(eventSource).FirstOrDefault();
            return e;
        }
        public static void SetDefaultEvent(Type eventSource, string eventName)
        {
            if (eventSource == null)
            {
                throw new ArgumentNullException(nameof(eventSource));
            }

            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentNullException(nameof(eventName));
            }
            var eventInfo = eventSource.GetEvent(eventName) 
                            ?? throw new InvalidOperationException($"Can not find event {eventName} in type of {eventSource.Name}.");
            _DefaultEvent[eventSource] = eventInfo;
        }
        public static void ClearDefaultEvent(Type eventSource)
        {
            if (eventSource == null)
            {
                throw new ArgumentNullException(nameof(eventSource));
            }

            _DefaultEvent.Remove(eventSource);
        }
        public static void ClearDefaultEvents()
        {
            _DefaultEvent.Clear();
        }
        public static EventInfo GetEventOrDefault(Type eventSource, string eventName)
        {
            if (eventSource == null)
            {
                throw new ArgumentNullException(nameof(eventSource));
            }

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
        static PropertyInfo[] GetProperties(Type type) => _Properties.GetValue(type, () => type.GetProperties());
        public static object GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            var type = obj.GetType();
            var property = GetProperties(type).SingleOrDefault(p => p.Name == propertyName);
            if (property == null) throw new InvalidOperationException($"Can not find property '{propertyName}' in type '{type.Name}'.");
            return property.GetValue(obj);
        }
        #endregion
        #region Commands
        static Dictionary<Type, PropertyInfo[]> _Commands = new Dictionary<Type, PropertyInfo[]>();
        static PropertyInfo GetCommandProperty(object commandSource, string commandName)
        {
            var commands = _Commands.GetValue(commandSource.GetType(), 
                                               () => GetProperties(commandSource.GetType()).Where
                                                        (p => p.PropertyType.IsAssignableFrom(typeof(ICommand))).ToArray());
            return commands.FirstOrDefault(p => p.Name == commandName);
        }
        static bool ExecuteCommand(object commandSource, string commandName)
        {
            var p = GetCommandProperty(commandSource, commandName);
            if (p == null) return false;
            var command = GetPropertyValue(commandSource, commandName) as ICommand;
            if (command.CanExecute(null)) command.Execute(null);
            return true;
        }
        static bool ExecuteCommand(object commandSource, string commandName, object parameter)
        {
            var p = GetCommandProperty(commandSource, commandName);
            if (p == null) return false;
            var command = GetPropertyValue(commandSource, commandName) as ICommand;
            if (command.CanExecute(parameter)) command.Execute(parameter);
            return true;
        }
        #endregion
    }
}
