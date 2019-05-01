using System;
using System.Collections.Generic;
using System.Text;

namespace MvvmTools.Design
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DesignTypeAttribute : Attribute
    {
        public Type Type { get; set; }

        public DesignTypeAttribute(Type type) => Type = type;
    }
}
