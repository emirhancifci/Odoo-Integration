using EventBus.Base.Events;
using GetCommerce.APP.Models.CommerceModels;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GetCommerce.APP.IntegrationEvents.Events
{
    public class GetCancelledOrdersIntegrationEvent : IntegrationEvent
    {
        [JsonProperty("CancelledOrders")]
        public List<Order> CancelledOrders { get; set; }


        public GetCancelledOrdersIntegrationEvent()
        {

        }
        [JsonConstructor]
        public GetCancelledOrdersIntegrationEvent(List<Order> cancelledOrders)
        {
            CancelledOrders = cancelledOrders;
        }
    }
}
