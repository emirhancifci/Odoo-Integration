using EventBus.Base.Events;
using GetCommerce.APP.Models.CommerceModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetCommerce.APP.IntegrationEvents.Events
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
