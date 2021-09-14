using EventBus.Base;
using EventBus.Base.Abstractions;
using EventBus.Base.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EventBus.Service.Helpers;
using Newtonsoft.Json;
using EventBus.Service.IntegrationEventService.SubscriptionManagers;

namespace EventBus.Service.IntegrationEventService.Events
{
    public abstract class BaseEventBus<T> : IEventBus
    {
        #region fields

        protected readonly IServiceProvider _serviceProvider;
        protected readonly IEventBusSubscriptionManager _eventBusSubscriptionManager;
        protected readonly ILogger<T> _logger;
        protected EventBusConfig EventBusConfig { get; set; }

        #endregion

        #region ctors

        protected BaseEventBus(
            EventBusConfig eventBusConfig,
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            EventBusConfig = eventBusConfig;
            _eventBusSubscriptionManager = new InMemoryEventBusSubscriptionManager(ProcessEventName);
            _logger = serviceProvider.GetRequiredService<ILogger<T>>();
        }

        #endregion


        #region local methods
        public virtual string ProcessEventName(string eventName)
        {
            if (EventBusConfig.DeleteEventPrefix)
                eventName = eventName.Replace(EventBusConfig.EventNamePrefix, "");

            if (EventBusConfig.DeleteEventSuffix)
                eventName = eventName.Replace(EventBusConfig.EventNameSuffix, "");
            //eventName = eventName.TrimEnd(EventBusConfig.EventNameSuffix.ToArray());

            return eventName;
        }

        public virtual string GetSubscriptionName(string eventName)
        {
            return $"{EventBusConfig.SubscriberClientAppName}.{ProcessEventName(eventName)}";
        }


        public async Task<bool> ProcessEventAsync(string eventName, string message)
        {
            eventName = ProcessEventName(eventName);

            bool processed = false;

            if (_eventBusSubscriptionManager.HasSubscriptionsForEvent(eventName))
            {
                var subscriptions = _eventBusSubscriptionManager.GetHandlersForEvent(eventName);

                using (var scope = _serviceProvider.CreateScope())
                {
                    foreach (var subscription in subscriptions)
                    {
                        var handler = _serviceProvider.GetService(subscription.HandlerType);
                        if (handler == null) _logger.LogWarning($"{subscription.HandlerType.Name} : {LogMessages.WARNING_MESSAGE_HANDLER_TYPE_IS_NOT_FOUND}");

                        var eventType = _eventBusSubscriptionManager.GetEventTypeByName($"{EventBusConfig.EventNamePrefix}{eventName}{EventBusConfig.EventNameSuffix}");
                        var integrationEvent = JsonConvert.DeserializeObject(message, eventType);

                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent }); // integration event handle methodunun parametresi için gerekli
                    }
                    processed = true;
                }
            }
            return processed;

        }

        #endregion


        #region implemantations
        public abstract void Publish(IntegrationEvent @event);

        public abstract void Subcribe<TEvent, THandler>()
            where TEvent : IntegrationEvent
            where THandler : IIntegrationEventHandler<TEvent>;

        public abstract void UnSubcribe<TEvent, THandler>()
            where TEvent : IntegrationEvent
            where THandler : IIntegrationEventHandler<TEvent>;
        #endregion



        #region dispose
        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
