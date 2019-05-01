using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using MvvmTools.Core;
using MvvmTools.EventBinding;

namespace MvvmTools.WPF
{
    public class PlatformWPF : PlatformBase
    {
        protected override object GetContext(object element)
        {
            var frameworkElement = (element as FrameworkElement) ?? throw new InvalidOperationException("Must be used on FrameworkElement");
            return frameworkElement.DataContext;
        }
        protected override object GetElement(object element, string name)
        {
            var uiElement = (element as FrameworkElement) ?? throw new InvalidOperationException("Must be used on FrameworkElement");
            return FindByName(name, uiElement); ;
        }
        object FindByName(string name, FrameworkElement element)
        {
            if (element == null) return null;
            var target = element.FindName(name);
            if (target != null) return target;
            if (element.Parent is FrameworkElement parent) return FindByName(name, parent);
            if (element.TemplatedParent is FrameworkElement templateParent) return FindByName(name, templateParent);
            if (VisualTreeHelper.GetParent(element) is FrameworkElement visualParent) return FindByName(name, visualParent);
            return null;
        }
        protected override object GetResource(object element, string name)
        {
            var uiElement = (element as FrameworkElement) ?? throw new InvalidOperationException("Must be used on FrameworkElement");
            return uiElement.FindResource(name); ;
        }
        protected override bool IsDesignMode => DesignerProperties.GetIsInDesignMode(new DependencyObject());
    }

    [TypeConverter(typeof(EventBindingsCollectionConverter))]
    public class EventBindingsCollection : EventBindingCollectionBase<PlatformWPF> { }

    public class EventBindingsCollectionConverter : EventBindingCollectionConverterBase<EventBindingsCollection> { }

    public static class Events
    {
        public static EventBindingsCollection GetBindings(DependencyObject obj) => (EventBindingsCollection)obj.GetValue(BindingsProperty);
        public static void SetBindings(DependencyObject obj, EventBindingsCollection value) => obj.SetValue(BindingsProperty, value);
        public static readonly DependencyProperty BindingsProperty =
            DependencyProperty.RegisterAttached("Bindings", typeof(EventBindingsCollection), typeof(Events), 
                new PropertyMetadata(null, BindingsChanged));

        private static void BindingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d == null) throw new ArgumentNullException(nameof(d));
            if (e.OldValue is EventBindingsCollection oldValue) oldValue.EventSource = null;
            if (e.NewValue is EventBindingsCollection newValue) newValue.EventSource = d;
        }
    }
}
