using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MvvmTools.XForms
{
    public enum RuntimeBehaviors
    {
        OrignalValue,
        TypeCreate,
        Null
    }
    public class DesignInstance : IMarkupExtension
    {
        [TypeConverter(typeof(TypeTypeConverter))]
        public Type Type { get; set; }
        public bool IsCreateList { get; set; }
        public int ItemsCount { get; set; } = 3;
        public int Depth { get; set; } = 2;
        public RuntimeBehaviors RuntimBehavior { get; set; } = RuntimeBehaviors.OrignalValue;

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (DesignMode.IsDesignModeEnabled)
            {
                if (Type == null) throw new ArgumentNullException(nameof(Type));
                try
                {
                    return Design.Data.Create(Type, Depth, IsCreateList, ItemsCount);
                }
                catch (Exception)
                {
                }
            }
            if (Type == null) return null;
            switch (RuntimBehavior)
            {
                case RuntimeBehaviors.OrignalValue:
                    var valueProvider = serviceProvider?.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
                    if (valueProvider?.TargetObject != null && valueProvider.TargetProperty != null)
                    {
                        if (valueProvider.TargetProperty is BindableProperty property)
                        {
                            if (valueProvider.TargetObject is BindableObject bindable) return bindable.GetValue(property);
                        }
                        else if (valueProvider.TargetProperty is PropertyInfo info)
                        {
                            return info.GetValue(valueProvider.TargetObject);
                        }
                    }
                    break;
                case RuntimeBehaviors.TypeCreate:
                    return Activator.CreateInstance(Type);
                case RuntimeBehaviors.Null:
                default:
                    return null;
            }
            return null;
        }
    }
}
