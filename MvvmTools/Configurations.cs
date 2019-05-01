using MvvmTools.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MvvmTools
{
    public static class Configurations
    {
        public static class Reflaction
        {
            public static bool HasDefaultEvent(Type type) => ReflectionCash.HasDefaultEvent(type);
            public static void SetDefaultEvent(Type type, string eventName) => ReflectionCash.SetDefaultEvent(type, eventName);
            public static bool SetDefaultEventIfNotExist(Type type, string eventName) => ReflectionCash.SetDefaultEventIfNotExist(type, eventName);
            public static void ClearDefaultEvent(Type type) => ReflectionCash.ClearDefaultEvent(type);
            public static void ClearDefaultEvents() => ReflectionCash.ClearDefaultEvents();
        }
        public static class Design
        {
            public static void MapType(Type type, Type mappedType) => MvvmTools.Design.Data.typesMaps[type] = mappedType;
            public static void MapType<TData, TDesign>() => MvvmTools.Design.Data.typesMaps[typeof(TData)] = typeof(TDesign);
            public static bool ClearMapType(Type type) => MvvmTools.Design.Data.typesMaps.Remove(type);
            public static bool ClearMapType<T>() => MvvmTools.Design.Data.typesMaps.Remove(typeof(T));
            public static void ClearMapTypeS() => MvvmTools.Design.Data.typesMaps.Clear();
        }
        public static class Logging
        {
            static Action<string, string> _logAction;
            public static Action<string,string> LogAction
            {
                get
                {
#if DEBUG
                    if (_logAction == null)
                        _logAction = (string message, string category) => Debug.WriteLine(message, category);
#endif
                    return _logAction;
                }
                set => _logAction = value;
            }

            public static bool EnableLogging { get; set; } = true;

            public static void Write(object obj, object message, string category)
            {
                if (!EnableLogging || LogAction == null) return;
                var sObject = (obj == null) ? "" : $"[{obj.GetType().Name} - {obj.GetHashCode().ToString()}]: ";
                var sMessage = message == null ? "" : message.ToString();
                LogAction(sObject + sMessage, category);
            }
        }
    }
}
