using EventBus.Base.Events;
using Newtonsoft.Json;
using System.Collections.Generic;
using ToErp.APP.Models;

namespace ToErp.APP.IntegrationEvents.Events
{
    public class GetOrdersIntegrationEvent : IntegrationEvent
    {
        [JsonProperty("Orders")]
        public List<Order> Orders { get; set; }

        public GetOrdersIntegrationEvent()
        {
            Orders ??= new List<Order>();
        }
        [JsonConstructor]
        public GetOrdersIntegrationEvent(List<Order> orders)
        {
            Orders = orders;
        }
    }
}
