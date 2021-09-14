using EventBus.Base.Abstractions;
using Microsoft.Extensions.Logging;
using Odoo.Concrete;
using System;
using System.Threading.Tasks;
using ToErp.APP.IntegrationEvents.Events;
using ToErp.APP.Integrations.Abstractions;

namespace ToErp.APP.IntegrationEvents.Handlers
{
    public class CheckPriceIntegrationEventHandler : IIntegrationEventHandler<CheckPriceIntegrationEvent>
    {
        private readonly IErpIntegration _erpIntegration;
        private readonly ILogger<CheckPriceIntegrationEventHandler> _logger;

        public CheckPriceIntegrationEventHandler(IErpIntegration erpIntegration, ILogger<CheckPriceIntegrationEventHandler> logger)
        {
            _erpIntegration = erpIntegration;
            _logger = logger;
        }

        public Task Handle(CheckPriceIntegrationEvent @event)
        {
            ProcessCheckPrice(@event);

            return Task.CompletedTask;
        }

        private void ProcessCheckPrice(CheckPriceIntegrationEvent @event)
        {
            bool isPriceTrue = false;

            var order = _erpIntegration.GetOrderFromErp<RpcRecord>(@event.OrderId.ToString());
            var amountTotal = order.GetField("amount_total").Value;

            if (amountTotal != null)
            {
                decimal odooPrice = 0;

                var wasParsed = decimal.TryParse(amountTotal.ToString(), out odooPrice);

                if (wasParsed)
                {
                    isPriceTrue = CheckPrice(@event.CommerceTotalPrice, odooPrice);
                }

            }

            order.SetFieldValue("x_price_check", isPriceTrue ? "1" : "0");
            order.Save();
        }


        private bool CheckPrice(decimal? commercePrice, decimal? erpPrice)
        {
            if (!commercePrice.HasValue || !erpPrice.HasValue)
            {
                return false;
            }
            decimal roolCommercePrice = Math.Ceiling(commercePrice.Value);
            decimal roolErpPrice = Math.Ceiling(erpPrice.Value);

            if (roolCommercePrice == roolErpPrice)
            {
                return true;
            }

            return false;
        }
    }
}
