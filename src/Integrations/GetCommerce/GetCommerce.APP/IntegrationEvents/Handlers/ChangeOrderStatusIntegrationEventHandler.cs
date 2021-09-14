using EventBus.Base.Abstractions;
using EventBus.Base.Events;
using GetCommerce.APP.IntegrationEvents.Events;
using GetCommerce.APP.Integrations.Abstractions;
using GetCommerce.APP.Models.CommerceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetCommerce.APP.IntegrationEvents.Handlers
{
    public class ChangeOrderStatusIntegrationEventHandler : IIntegrationEventHandler<ChangeOrderStatusIntegrationEvent>
    {
        private readonly ICommerceIntegration _commerceIntegration;
        private readonly IEventBus _eventBus;
        public ChangeOrderStatusIntegrationEventHandler(IEventBus eventBus, ICommerceIntegration commerceIntegration)
        {
            _commerceIntegration = commerceIntegration;
            _eventBus = eventBus;
        }
        public Task Handle(ChangeOrderStatusIntegrationEvent @event)
        {
            _commerceIntegration.ChangeOrderStatus(@event.Order, @event.OrderStatus);
            if (@event.OrderStatus == OrderStatus.Aktarildi)
            {
                _eventBus.Publish(new CheckPriceIntegrationEvent(@event.Order.OrderSubTotal,@event.Order.OrderID));
            }
            return Task.CompletedTask;
        }
    }
}
