using EventBus.Base.Events;
using Newtonsoft.Json;
using System.Collections.Generic;
using ToErp.APP.Models;

namespace ToErp.APP.IntegrationEvents.Events
{
    public class GetSentOrdersIntegrationEvent : IntegrationEvent
    {
        [JsonProperty("Orders")]
        public List<Order> SentOrders { get; set; }

        public GetSentOrdersIntegrationEvent()
        {

        }
        [JsonConstructor]
        public GetSentOrdersIntegrationEvent(List<Order> sentOrders)
        {
            SentOrders = sentOrders;
        }
    }
}
