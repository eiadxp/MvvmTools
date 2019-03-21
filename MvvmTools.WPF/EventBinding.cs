using System;
using System.ComponentModel;
using System.Windows;
using MvvmTools.Core;
using MvvmTools.EventBinding;

namespace MvvmTools.WPF
{
    public class ValueProvider : ValueProviderBase
    {
        protected override object GetContext()
        {
            var element = (UIElement as FrameworkElement) ?? throw new InvalidOperationException("Must be used on FrameworkElement");
            return element.DataContext;
        }
        protected override object GetElement(string name)
        {
            var element = (UIElement as FrameworkElement) ?? throw new InvalidOperationException("Must be used on FrameworkElement");
            return element.FindName(name); ;
        }
        protected override object GetResource(string name)
        {
            var element = (UIElement as FrameworkElement) ?? throw new InvalidOperationException("Must be used on FrameworkElement");
            return element.FindResource(name); ;
        }
    }

    [TypeConverter(typeof(EventBindingsCollectionConverter))]
    public class EventBindingsCollection : EventBindingCollectionBase<ValueProvider> { }

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
