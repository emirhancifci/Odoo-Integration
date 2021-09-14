using GetCommerce.APP.Models.CommerceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetCommerce.APP.Integrations.Abstractions
{
    public interface ICommerceIntegration : IDisposable
    {
        List<Order> GetOrders<TStatus>(TStatus stateId);

        void ChangeOrderStatus<TModel, TStatus>(TModel order, TStatus status);
    }
}
