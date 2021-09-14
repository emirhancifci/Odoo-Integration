using EventBus.Base.Events;
using Newtonsoft.Json;

namespace GetCommerce.APP.IntegrationEvents.Events
{
    public class CheckPriceIntegrationEvent : IntegrationEvent
    {
        [JsonProperty]
        public decimal CommerceTotalPrice { get; set; }

        public int OrderId { get; set; }

        public CheckPriceIntegrationEvent()
        {

        }

        [JsonConstructor]
        public CheckPriceIntegrationEvent(decimal commerceTotalPrice,int orderId)
        {
            CommerceTotalPrice = commerceTotalPrice;
            OrderId = orderId;
        }
    }
}
