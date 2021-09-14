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
    public class GetCancelledIntegrationEventHandler : IIntegrationEventHandler<GetCancelledOrdersIntegrationEvent>
    {

        private readonly IErpIntegration _erpIntegration;
        private readonly ILogger<GetCancelledIntegrationEventHandler> _logger;
        private readonly IEventBus _eventBus;

        public GetCancelledIntegrationEventHandler(IEventBus eventBus, IErpIntegration erpIntegration, ILogger<GetCancelledIntegrationEventHandler> logger)
        {
            _erpIntegration = erpIntegration;
            _logger = logger;
            _eventBus = eventBus;
        }
        public Task Handle(GetCancelledOrdersIntegrationEvent @event)
        {
            ProcessOrder(@event);

            return Task.CompletedTask;
        }


        private void ProcessOrder(GetCancelledOrdersIntegrationEvent @event)
        {
            foreach (var order in @event.CancelledOrders)
            {
                try
                {
                    _erpIntegration.ChangeOrderStatus<OdooOrderStatus>(order.OrderID.ToString(), OdooOrderStatus.cancel);
                  //  _erpIntegration.ChangeShipmentStatus<OrderStatus>(order.OrderID.ToString(), OrderStatus.İptalEdildi);
                    _eventBus.Publish(new ChangeOrderStatusIntegrationEvent(order, OrderStatus.IptalAktarildi));

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
