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
    public class ChangeOrderStatusIntegrationEvent : IntegrationEvent
    {
        [JsonProperty]
        public Order Order { get; set; }

        [JsonProperty]
        public OrderStatus OrderStatus { get; set; }



        public ChangeOrderStatusIntegrationEvent()
        {

        }

        [JsonConstructor]
        public ChangeOrderStatusIntegrationEvent(Order order, OrderStatus orderStatus)
        {
            Order = order;
            OrderStatus = orderStatus;
        }
    }
}
