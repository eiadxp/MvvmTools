using MvvmTools.Core;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MvvmTools.EventBinding
{
    public class EventBinding<TValueProvider> where TValueProvider : ValueProviderBase, new()
    {
        #region Constructors
        public EventBinding()
        {
            this.LogWrite("EventBinding created with default constructor.");
        }
        public EventBinding(object eventSource, string eventString)
        {
            this.LogWrite("EventBinding created with (object, string) constructor.");
            EventSource = eventSource;
            EventSrting = eventString;
        }
        #endregion
        #region private methods
        /// <summary>
        /// It will throw an exception if the class already subscribed to event, otherwise it will return <c>value</c>.
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="value">Value to be returned</param>
        /// <param name="property">Name of the property called this method.</param>
        /// <returns>If the class did not subscribed to an event it will return <c>value</c>, otherwise it will throw an exception.</returns>
        /// <exception cref="InvalidOperationException">The current instance is already subscribed to an event.</exception>
        /// <remarks>
        /// Some properties like <see cref="EventSrting"/> and <see cref="EventSource"/> should not be changed if the current instance
        /// is already subscribed to an event.
        /// </remarks>
        T SetIfNotSubscribed<T>(T value, [CallerMemberName] string property = "")
        {
            if (IsSubscribed)
            {
                this.LogWrite("SetIfNotSubscribed failed.");
                throw new InvalidOperationException($"Can not change property '{property}' while you are subscribed to the event./nPlease remove the event handler first.");
            }

            this.LogWrite("SetIfNotSubscribed passed.");
            return value;
        }
        #endregion
        #region Properties fields
        private string _eventString;
        private object _eventSource;
        private readonly MethodParser<TValueProvider> _method = new MethodParser<TValueProvider>();
        #endregion
        #region Properties
        public string EventSrting { get => _eventString; set => _eventString = SetIfNotSubscribed(value); }
        public object EventSource { get => _eventSource; set => _eventSource = SetIfNotSubscribed(value); }
        public bool IsSubscribed { get; private set; }

        public Delegate Handler { get; private set; }
        #endregion
        #region Handlers
        public static Delegate CreateEventHandler(Type eventHandlerType, EventBinding<TValueProvider> binding)
        {
            var handlerParameters = ReflectionCash.GetHandlerParameter(eventHandlerType).Select(p => Expression.Parameter(p.ParameterType)).ToList();
            Expression methodSender = handlerParameters[0];
            Expression methodArgs = handlerParameters[1];
            var methodCall = Expression.Call(EventHandlerInfo, new Expression[] { Expression.Constant(binding), methodSender, methodArgs });
            var handler = Expression.Lambda(eventHandlerType, methodCall, handlerParameters).Compile();
            return handler;
        }

        static readonly MethodInfo EventHandlerInfo = typeof(EventBinding<TValueProvider>).GetMethod(nameof(EventHandler), BindingFlags.Static | BindingFlags.NonPublic);
        static void EventHandler(EventBinding<TValueProvider> binding, object sender, object args)
        {
            binding._method.ExecuteFromEvent(sender, args);
        }
        #endregion
        EventInfo subscribedEvent;
        Delegate subscribedDelegate;
        internal bool SubscribeToEvent()
        {
            this.LogWrite($"'{nameof(SubscribeToEvent)}' called.");
            if (Handler != null || IsSubscribed)
            {
                this.LogWrite($"'{nameof(SubscribeToEvent)}' canceled (already subscribed).");
                return false;
            }

            if (string.IsNullOrWhiteSpace(EventSrting))
            {
                this.LogWrite($"'{nameof(SubscribeToEvent)}' canceled (Empty event string).");
                return false;
            }

            if (EventSource == null)
            {
                this.LogWrite($"'{nameof(SubscribeToEvent)}' canceled (Null event source).");
                return false;
            }

            var i = EventSrting.IndexOf('=');
            var eventName = (i > 0) ? EventSrting.Substring(0, i).Trim() : "";
            subscribedEvent = ReflectionCash.GetEventOrDefault(EventSource.GetType(), eventName) ?? throw new InvalidOperationException("Can not find event.");
            _method.UIElement = EventSource;
            _method.Text = EventSrting.Substring(i + 1);
            subscribedDelegate = CreateEventHandler(subscribedEvent.EventHandlerType, this);
            subscribedEvent.AddEventHandler(EventSource, subscribedDelegate);
            IsSubscribed = true;
            this.LogWrite($"'{nameof(SubscribeToEvent)}' passed.");
            return true;
        }
        internal bool UnubscribeFromEvent()
        {
            if (Handler == null || !IsSubscribed || subscribedEvent == null || subscribedDelegate == null) return false;
            if (string.IsNullOrWhiteSpace(EventSrting)) return false;
            if (EventSource == null) return false;
            subscribedEvent.RemoveEventHandler(EventSource, subscribedDelegate);
            subscribedEvent = null;
            IsSubscribed = false;
            return true;
        }
    }
}
