using MvvmTools;
using MvvmTools.Core;
using MvvmTools.EventBinding;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

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
            return FindByName(name, element);
        }
        object FindByName(string name, Element element)
        {
            if (element == null) return null;
            return GetNameScope(element)?.FindByName(name) ?? FindByName(name, element.Parent);
        }
        // Code taken from Xamarin.Forms source code.
        INameScope GetNameScope(Element element)
        {
            do
            {
                var ns = NameScope.GetNameScope(element);
                if (ns != null) return ns;
            } while ((element = element.RealParent) != null);
            return null;
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
        protected override void Initialize()
        {
            base.Initialize();
            Configurations.Reflaction.SetDefaultEventIfNotExist(typeof(Button), nameof(Button.Clicked));
            Configurations.Reflaction.SetDefaultEventIfNotExist(typeof(WebView), nameof(WebView.Navigated));
            Configurations.Reflaction.SetDefaultEventIfNotExist(typeof(SearchBar), nameof(SearchBar.SearchButtonPressed));
            Configurations.Reflaction.SetDefaultEventIfNotExist(typeof(Slider), nameof(Slider.ValueChanged));
            Configurations.Reflaction.SetDefaultEventIfNotExist(typeof(Stepper), nameof(Stepper.ValueChanged));
            Configurations.Reflaction.SetDefaultEventIfNotExist(typeof(Switch), nameof(Switch.Toggled));
            Configurations.Reflaction.SetDefaultEventIfNotExist(typeof(DatePicker), nameof(DatePicker.DateSelected));
            Configurations.Reflaction.SetDefaultEventIfNotExist(typeof(TimePicker), nameof(TimePicker.PropertyChanged));
            Configurations.Reflaction.SetDefaultEventIfNotExist(typeof(Entry), nameof(Entry.Completed));
            Configurations.Reflaction.SetDefaultEventIfNotExist(typeof(Editor), nameof(Editor.Completed));
            Configurations.Reflaction.SetDefaultEventIfNotExist(typeof(ListView), nameof(ListView.ItemSelected));
            Configurations.Reflaction.SetDefaultEventIfNotExist(typeof(Picker), nameof(Picker.SelectedIndexChanged));
        }
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
