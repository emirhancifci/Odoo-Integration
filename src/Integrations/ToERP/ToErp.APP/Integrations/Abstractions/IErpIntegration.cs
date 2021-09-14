using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToErp.APP.Integrations.Abstractions
{
    public interface IErpIntegration : IDisposable
    {
        public void SetOrderToErp<TEntity>(TEntity entity);

        public T GetOrderFromErp<T>(string orderId) where T : class;

        void ChangeOrderStatus<TModel,TStatus>(TModel order, TStatus status);

        void ChangeShipmentStatus<TModel, TStatus>(TModel order, TStatus status);

    }
}
