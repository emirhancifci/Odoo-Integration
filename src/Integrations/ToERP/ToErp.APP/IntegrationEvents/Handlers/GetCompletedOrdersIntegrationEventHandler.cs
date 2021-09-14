using EventBus.Base.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ToErp.APP.IntegrationEvents.Events;
using ToErp.APP.Integrations.Abstractions;
using ToErp.APP.Models;
using ToErp.APP.Models.OdooModel;

namespace ToErp.APP.IntegrationEvents.Handlers
{
    public class GetCompletedOrdersIntegrationEventHandler : IIntegrationEventHandler<GetCompletedOrdersIntegrationEvent>
    {
        private readonly IErpIntegration _erpIntegration;
        private readonly ILogger<GetCompletedOrdersIntegrationEventHandler> _logger;
        private readonly IEventBus _eventBus;

        public GetCompletedOrdersIntegrationEventHandler(IEventBus eventBus, IErpIntegration erpIntegration, ILogger<GetCompletedOrdersIntegrationEventHandler> logger)
        {
            _erpIntegration = erpIntegration;
            _logger = logger;
            _eventBus = eventBus;
        }
        public Task Handle(GetCompletedOrdersIntegrationEvent @event)
        {
            ProcessOrder(@event);

            return Task.CompletedTask;
        }


        private void ProcessOrder(GetCompletedOrdersIntegrationEvent @event)
        {
            foreach (var order in @event.CompletedOrders)
            {
                try
                {
                    _erpIntegration.ChangeOrderStatus<OdooOrderStatus>(order.OrderID.ToString(), OdooOrderStatus.done);
                    _erpIntegration.ChangeShipmentStatus<OrderStatus>(order.OrderID.ToString(), OrderStatus.Tamamlandi);

                    _eventBus.Publish(new ChangeOrderStatusIntegrationEvent(order, OrderStatus.TamamlandiAktarildi));
                    
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
