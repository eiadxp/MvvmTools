using MvvmTools.Core;
using MvvmTools.EventBinding;
using System;
using Xamarin.Forms;

namespace MvvmTools.XForms
{
    public class ValueProvider : ValueProviderBase
    {
        protected override object GetContext()
        {
            var element = (UIElement as BindableObject) ?? throw new InvalidOperationException("Must be used on BindableObject");
            return element.BindingContext;
        }
        protected override object GetElement(string name)
        {
            var element = (UIElement as Element) ?? throw new InvalidOperationException("Must be used on Element");
            return element.FindByName(name);
        }
        protected override object GetResource(string name)
        {
            var element = (UIElement as VisualElement) ?? throw new InvalidOperationException("Must be used on Element");
            return GetResource(element, name);
        }
        object GetResource(VisualElement element, string key)
        {
            if(element == null)
            {
                var resources = Application.Current?.Resources;
                if (resources == null) return null;
                if (resources.TryGetValue(key, out object globalValue)) return globalValue;
                return null;
            }
            if (element.Resources != null && element.Resources.TryGetValue(key, out object value)) return value;
            return GetResource(element.Parent as VisualElement, key);
        }
    }
    [TypeConverter(typeof(EventBindingsCollectionConverter))]
    public class EventBindingsCollection : EventBindingCollectionBase<ValueProvider>
    {

    }
    public class EventBindingsCollectionConverter : EventBindingCollectionConverterBase<EventBindingsCollection>
    {

    }
    public static class Events
    {
        public static EventBindingsCollection GetBindings(BindableObject obj) => (EventBindingsCollection)obj.GetValue(BindingsProperty);
        public static void SetBindings(BindableObject obj, EventBindingsCollection value) => obj.SetValue(BindingsProperty, value);
        public static readonly BindableProperty BindingsProperty =
            BindableProperty.CreateAttached("Bindings", typeof(EventBindingsCollection), typeof(Events), null, 
                propertyChanged: BindingsChanged);

        private static void BindingsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable == null) throw new ArgumentNullException(nameof(bindable));
            if (oldValue is EventBindingsCollection oldCollection) oldCollection.EventSource = null;
            if (newValue is EventBindingsCollection newCollection) newCollection.EventSource = bindable;
        }
    }
}
