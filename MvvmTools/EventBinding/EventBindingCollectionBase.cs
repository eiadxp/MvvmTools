using MvvmTools.Core;
using System.Collections.ObjectModel;

namespace MvvmTools.EventBinding
{
    public class EventBindingCollectionBase<TValueProvider> : Collection<EventBinding<TValueProvider>> , IAddFromString
        where TValueProvider : ValueProviderBase , new ()
    {
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
                        item.UnubscribeFromEvent();
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
            }
        }

        protected override void ClearItems()
        {
            if (EventSource != null)
            {
                foreach (var item in this)
                {
                    item.UnubscribeFromEvent();
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
                a.UnubscribeFromEvent();
            }
            else
            {
                base.RemoveItem(index);
            }
        }
        protected override void InsertItem(int index, EventBinding<TValueProvider> item)
        {
            base.InsertItem(index, item);
            if (EventSource != null)
            {
                item.EventSource = EventSource;
                item.SubscribeToEvent();
            }
        }
        protected override void SetItem(int index, EventBinding<TValueProvider> item)
        {
            if (EventSource != null)
            {
                var a = this[index];
                base.SetItem(index, item);
                a.UnubscribeFromEvent();
                item.EventSource = EventSource;
                item.SubscribeToEvent();
            }
            else
            {
                base.SetItem(index, item);
            }
        }

        public void AddFromString(string text) => Add(new EventBinding<TValueProvider>(null, text));
        public void AddRangeFromString(string text)
        {
            foreach (var item in text.Split(',')) AddFromString(item);
        }
    }
    public interface IAddFromString
    {
        void AddFromString(string text);
        void AddRangeFromString(string text);
    }
}
