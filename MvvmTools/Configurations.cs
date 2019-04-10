using MvvmTools.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MvvmTools
{
    public static class Configurations
    {
        public static class Reflaction
        {
            public static void SetDefaultEvent(Type type, string eventName) => ReflectionCash.SetDefaultEvent(type, eventName);
            public static void ClearDefaultEvent(Type type) => ReflectionCash.ClearDefaultEvent(type);
            public static void ClearDefaultEvents() => ReflectionCash.ClearDefaultEvents();
        }
    }
}
