using EventBus.Base.Events;
using Newtonsoft.Json;
using System.Collections.Generic;
using ToErp.APP.Models;

namespace ToErp.APP.IntegrationEvents.Events
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
