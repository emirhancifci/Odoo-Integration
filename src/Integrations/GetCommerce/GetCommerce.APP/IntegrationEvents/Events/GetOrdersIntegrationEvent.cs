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
    public class GetOrdersIntegrationEvent : IntegrationEvent
    {

        [JsonProperty("Orders")]
        public List<Order> Orders { get; set; }

        public GetOrdersIntegrationEvent()
        {

        }
        [JsonConstructor]
        public GetOrdersIntegrationEvent(List<Order> orders)
        {
            Orders = orders;
        }
    }
}
