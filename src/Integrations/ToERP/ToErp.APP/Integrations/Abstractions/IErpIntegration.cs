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

        void ChangeOrderStatus<TStatus>(string orderId, TStatus status);

        void ChangeShipmentStatus<TStatus>(string orderId, TStatus status);

    }
}
