using EventBus.Base;
using EventBus.Base.Abstractions;
using EventBus.Base.Events;
using EventBus.Service.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Service.IntegrationEventService.SubscriptionManagers
{
    public class InMemoryEventBusSubscriptionManager : IEventBusSubscriptionManager
    {
        #region fields

        private readonly Func<string, string> _eventNameGetter;
        private readonly List<Type> _eventTypes;
        private readonly Dictionary<string, List<SubscriptionInfo>> _handlers;
        #endregion

        #region ctors
        public InMemoryEventBusSubscriptionManager(Func<string, string> eventNameGetter)
        {
            _eventTypes = new();
            _handlers = new();
            _eventNameGetter = eventNameGetter;
        }

        #endregion

        #region implematation

        #region properties and events
        public bool IsEmpty => !_handlers.Keys.Any();

        public event EventHandler<string> OnEventRemoved;

        #endregion

        #region methods
        public void AddSubscription<TEvent, THandler>()
            where TEvent : IntegrationEvent
            where THandler : IIntegrationEventHandler<TEvent>
        {
            var eventName = GetEventKey<TEvent>();

            AddSubscription(typeof(THandler),eventName);

            if (!_eventTypes.Contains(typeof(TEvent)))
            {
                _eventTypes.Add(typeof(TEvent));
            }
        }

        public void Clear() => _handlers.Clear();

        public string GetEventKey<T>()
        {
            string eventName = typeof(T).Name;

            return _eventNameGetter(eventName);
        }

        public Type GetEventTypeByName(string eventName) => _eventTypes.SingleOrDefault(t => t.Name == eventName);

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent<TEvent>() where TEvent : IntegrationEvent
        {
            var key = GetEventKey<TEvent>();

            return GetHandlersForEvent(key);
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName) => _handlers[eventName];

        public bool HasSubscriptionsForEvent<TEvent>() where TEvent : IntegrationEvent
        {
            var key = GetEventKey<TEvent>();

            return HasSubscriptionsForEvent(key);
        }


        public bool HasSubscriptionsForEvent(string eventName) => _handlers.ContainsKey(eventName);

        public void RemoveSubscription<TEvent, THandler>()
            where TEvent : IntegrationEvent
            where THandler : IIntegrationEventHandler<TEvent>
        {
            var handlerToRemove = FindSubscriptionToRemove<TEvent, THandler>();
            var eventName = GetEventKey<TEvent>();
            RemoveHandler(eventName, handlerToRemove);
        }

        #endregion

        #endregion

        #region private methods

        private void RaiseOnEventRemoved(string eventName)
        {
            var handler = OnEventRemoved;
            handler?.Invoke(this,eventName);
        }
        private void RemoveHandler(string eventName,SubscriptionInfo subscriptionInfo)
        {
            if (subscriptionInfo != null)
            {
                _handlers[eventName].Remove(subscriptionInfo);

                if (!_handlers[eventName].Any())
                {
                    _handlers.Remove(eventName);
                    var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
                    if (eventType != null)
                    {
                        _eventTypes.Remove(eventType);
                    }

                    RaiseOnEventRemoved(eventName);
                }
            }
        }
        private SubscriptionInfo FindSubscriptionToRemove<TEvent,THandler>() where TEvent:IntegrationEvent where THandler:IIntegrationEventHandler<TEvent>
        {
            var eventName = GetEventKey<TEvent>();
            return FindSubscriptionToRemove(eventName, typeof(THandler));
        }
        private SubscriptionInfo FindSubscriptionToRemove(string eventName,Type handlerType)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                return null;
            }
            return _handlers[eventName].SingleOrDefault(x => x.HandlerType == handlerType);
        }

        private void AddSubscription(Type handlerType,string eventName)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                _handlers.Add(eventName, new List<SubscriptionInfo>());
            }

            if (_handlers[eventName].Any(x=>x.HandlerType == handlerType))
            {
                throw new ArgumentException($"{handlerType.Name} {ExceptionMessages.ALREADY_EXISTS_HANDLER_TYPE} '{eventName}'", nameof(handlerType));
            }

            _handlers[eventName].Add(SubscriptionInfo.SetType(handlerType));
        }

        #endregion

        #region dispose
        public void Dispose()
        {
            GC.SuppressFinalize(_eventNameGetter);
            GC.SuppressFinalize(_eventTypes);
            GC.SuppressFinalize(_handlers);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
