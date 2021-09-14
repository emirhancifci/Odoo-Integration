using EventBus.Base.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ToErp.APP.IntegrationEvents.Events;
using ToErp.APP.Integrations.Abstractions;
using ToErp.APP.Models;

namespace ToErp.APP.IntegrationEvents.Handlers
{
    public class GetOrderIntegrationEventHandler : IIntegrationEventHandler<GetOrdersIntegrationEvent>
    {
        private readonly IErpIntegration _erpIntegration;
        private readonly ILogger<GetOrderIntegrationEventHandler> _logger;
        private readonly IEventBus _eventBus;

        public GetOrderIntegrationEventHandler(IEventBus eventBus, IErpIntegration erpIntegration, ILogger<GetOrderIntegrationEventHandler> logger)
        {
            _erpIntegration = erpIntegration;
            _logger = logger;
            _eventBus = eventBus;
        }
        public Task Handle(GetOrdersIntegrationEvent @event)
        {
            ProcessOrder(@event);

            return Task.CompletedTask;
        }


        private void ProcessOrder(GetOrdersIntegrationEvent @event)
        {
            foreach (var order in @event.Orders)
            {
                try
                {
                    _erpIntegration.SetOrderToErp<Order>(order);
                    _eventBus.Publish(new ChangeOrderStatusIntegrationEvent(order, OrderStatus.Aktarildi));
                    _logger.LogInformation($"{order.OrderID} Odoo'ya aktarıldı :");
                }
                catch (Exception exception)
                {
                    _logger.LogError($"{order.OrderID} Odoo'ya aktarılırken bir hata olustu : {exception.Message}");
                }
            }
        }
    }
}
