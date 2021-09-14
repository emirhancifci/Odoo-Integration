using EventBus.Base.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToErp.APP.Models;

namespace ToErp.APP.IntegrationEvents.Events
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
