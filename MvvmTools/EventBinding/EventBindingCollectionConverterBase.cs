﻿using MvvmTools.Core;
using System;
using System.ComponentModel;
using System.Globalization;

namespace MvvmTools.EventBinding
{
    public class EventBindingCollectionConverterBase<TEventBindingCollection> : TypeConverter
        where TEventBindingCollection : IAddFromString, new()
    {
        public EventBindingCollectionConverterBase() { }
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null || value.GetType() != typeof(string)) return base.ConvertFrom(context, culture, value);
            var s = ((string)value).Trim();
            var collection = new TEventBindingCollection();
            collection.AddRangeFromString(s);
            return collection;
        }
    }
}
