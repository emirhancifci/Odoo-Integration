using AutoMapper;
using EventBus.Base.Abstractions;
using GetCommerce.APP.IntegrationEvents.Events;
using GetCommerce.APP.Integrations.Abstractions;
using GetCommerce.APP.Models;
using Kmwsc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace GetCommerce.APP.Integrations.Commerce
{
    public class CommerceIntegration : ICommerceIntegration
    {
        private readonly ILogger<CommerceIntegration> _logger;
        private readonly EndpointAddress _address;  //Enpoint oluşturuluyor
        private readonly KobiMasterServiceOperationServiceClient _client;
        private readonly IMapper _mapper;
        private readonly IEventBus _eventBus;
        private bool _disposed = false;
        public CommerceIntegration(IOptions<CommerceConfig> options, IMapper mapper,ILogger<CommerceIntegration> logger,IEventBus eventBus)
        {
            _logger = logger;
            _address = new EndpointAddress(options.Value.EndPoint);
            _client = new KobiMasterServiceOperationServiceClient(
                KobiMasterServiceOperationServiceClient.EndpointConfiguration.WSHttpBinding_KobiMasterServiceOperationService,
                _address);
            _client.ClientCredentials.UserName.UserName = options.Value.UserName;
            _client.ClientCredentials.UserName.Password = options.Value.Password;
            _mapper = mapper;
            _eventBus = eventBus;
        }
        public List<Models.CommerceModels.Order> GetOrders<TStatus>(TStatus stateId)
        {
            int[] orderStateList = new int[1];

            orderStateList[0] = Convert.ToInt32(stateId);
            List<Order> ordersWithOrderDetail = new List<Order>();
            var result = _client.GetOrdersByStateAsync(orderStateList).GetAwaiter().GetResult();
            foreach (var item in result)
            {
                var order = _client.GetSingleOrderAsync(item.OrderID).GetAwaiter().GetResult();
                ordersWithOrderDetail.Add(order);
            }
            var detailOrders = ordersWithOrderDetail.ToArray();

            var orders = _mapper.Map<Order[],List<Models.CommerceModels.Order>>(detailOrders);
            
            return orders;

        }
        
        public void ChangeOrderStatus<TModel,TStatus>(TModel order,TStatus status)
        {
            Models.CommerceModels.Order commerceOrder = order as Models.CommerceModels.Order;
            var commerceStatus = ParseStatusToEnum(status);

            var result = _client.UpdateOrderStateAsync(commerceOrder.OrderID, Convert.ToInt32(status), _client.ClientCredentials.UserName.UserName).GetAwaiter().GetResult();

            if (result.IsFailed)
            {
                _logger.LogError($"An error occured during ChangeOrderStatus method process : {result.ReturnedMessage}");
                _eventBus.Publish(new ChangeOrderStatusIntegrationEvent(commerceOrder, commerceStatus));
            }
        }

        #region local methods
        private Models.CommerceModels.OrderStatus ParseStatusToEnum(object status)
        {
            Models.CommerceModels.OrderStatus orderStatus;
            var wasParsed = Enum.TryParse(status.ToString(), out orderStatus);
            if (wasParsed)
            {
                return orderStatus;
            }
            else
            {
                throw new Exception("An error occurred during Enum Parsing");
            }
        }

        #endregion
        #region dispose
        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {

            if (_disposed)
            {
                return;
            }

            GC.SuppressFinalize(_client);
            GC.SuppressFinalize(_address);
            GC.SuppressFinalize(this);

            _disposed = true;

        }

        #endregion

    }
}
