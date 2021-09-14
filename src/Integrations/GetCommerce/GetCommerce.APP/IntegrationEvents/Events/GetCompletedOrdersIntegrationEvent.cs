using EventBus.Base.Events;
using GetCommerce.APP.Models.CommerceModels;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GetCommerce.APP.IntegrationEvents.Events
{
    public class GetCompletedOrdersIntegrationEvent : IntegrationEvent
    {
        [JsonProperty("CompletedOrders")]
        public List<Order> CompletedOrders { get; set; }


        public GetCompletedOrdersIntegrationEvent()
        {

        }
        [JsonConstructor]
        public GetCompletedOrdersIntegrationEvent(List<Order> completedOrders)
        {
            CompletedOrders = completedOrders;
        }
    }
}
