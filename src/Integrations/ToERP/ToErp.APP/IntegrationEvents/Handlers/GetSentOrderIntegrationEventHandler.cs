using EventBus.Base.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ToErp.APP.IntegrationEvents.Events;
using ToErp.APP.Integrations.Abstractions;
using ToErp.APP.Models;

namespace ToErp.APP.IntegrationEvents.Handlers
{
    public class GetSentOrderIntegrationEventHandler : IIntegrationEventHandler<GetSentOrdersIntegrationEvent>
    {

        private readonly IErpIntegration _erpIntegration;
        private readonly ILogger<GetSentOrderIntegrationEventHandler> _logger;
        private readonly IEventBus _eventBus;

        public GetSentOrderIntegrationEventHandler(IEventBus eventBus, IErpIntegration erpIntegration, ILogger<GetSentOrderIntegrationEventHandler> logger)
        {
            _erpIntegration = erpIntegration;
            _logger = logger;
            _eventBus = eventBus;
        }
        public Task Handle(GetSentOrdersIntegrationEvent @event)
        {
            ProcessOrder(@event);

            return Task.CompletedTask;
        }

        private void ProcessOrder(GetSentOrdersIntegrationEvent @event)
        {
            foreach (var order in @event.SentOrders)
            {
                try
                {
                    _erpIntegration.ChangeShipmentStatus<OrderStatus>(order.OrderID.ToString(),OrderStatus.Gönderildi);

                   // _eventBus.Publish(new ChangeOrderStatusIntegrationEvent(order, OrderStatus.GönderildiAktarildi));
                }
                catch (Exception exception)
                {
                    _logger.LogError($"{order.OrderID} Odoo'ya aktarılırken bir hata olustu : {exception.Message}");
                }
                _logger.LogInformation($"{order.OrderID} Odoo'ya aktarıldı :");
            }
        }
    }
}
