using MvvmTools.Core;
using System.Collections.ObjectModel;

namespace MvvmTools.EventBinding
{
    public abstract class EventBindingCollectionBase<TPlatform> : Collection<EventBinding<TPlatform>> , IAddFromString
        where TPlatform : PlatformBase , new ()
    {
        private TPlatform _platform = new TPlatform();


        private object _eventSource;
        public object EventSource
        {
            get { return _eventSource; }
            set
            {
                if (_eventSource != null)
                {
                    foreach (var item in this)
                    {
                        item.UnsubscribeFromEvent();
                    }
                }
                _eventSource = value;
                if (_eventSource != null)
                {
                    foreach (var item in this)
                    {
                        item.EventSource = _eventSource;
                        item.SubscribeToEvent();
                    }
                }
                Validate();
            }
        }
        public bool AutoValidate { get; set; }
        #region Collection
        protected override void ClearItems()
        {
            if (EventSource != null)
            {
                foreach (var item in this)
                {
                    item.UnsubscribeFromEvent();
                }
            }
            base.ClearItems();
        }
        protected override void RemoveItem(int index)
        {
            if (EventSource != null)
            {
                var a = this[index];
                base.RemoveItem(index);
                a.UnsubscribeFromEvent();
            }
            else
            {
                base.RemoveItem(index);
            }
        }
        protected override void InsertItem(int index, EventBinding<TPlatform> item)
        {
            base.InsertItem(index, item);
            if (EventSource != null)
            {
                item.EventSource = EventSource;
                item.SubscribeToEvent();
                Validate();
            }
        }
        protected override void SetItem(int index, EventBinding<TPlatform> item)
        {
            if (EventSource != null)
            {
                var a = this[index];
                base.SetItem(index, item);
                a.UnsubscribeFromEvent();
                item.EventSource = EventSource;
                item.SubscribeToEvent();
                Validate();
            }
            else
            {
                base.SetItem(index, item);
            }
        }
        #endregion

        public void AddRangeFromString(string text)
        {
            AutoValidate = text.StartsWith("?");
            if (AutoValidate) text = text.Substring(1);
            foreach (var item in text.Split(','))
            {
                Add(new EventBinding<TPlatform>(null, item));
            }

            Validate();
        }
        void Validate()
        {
            if (EventSource == null) return;
            if (!AutoValidate) return;
            if (!_platform.IsDesignMode) return;
            foreach (var item in this)
            {
                item.Validate();
            }
        }
    }
    /// <summary>
    /// This interface is implemented by the <see cref="EventBindingCollectionBase{TValueProvider}"/> to be used by the converter 
    /// <see cref="EventBindingCollectionConverterBase{TEventBindingCollection}"/>.( Internal use only)
    /// </summary>
    public interface IAddFromString
    {
        /// <summary>
        /// Convert the text to a collection of <see cref="EventBinding{TValueProvider}"/> and add it to the
        /// </summary>
        /// <param name="text">Text to be converted to a collection of <see cref="EventBinding{TValueProvider}"/>.</param>
        void AddRangeFromString(string text);
    }
}
