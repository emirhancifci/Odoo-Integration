using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Odoo.Concrete;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ToErp.APP.Integrations.Abstractions;
using ToErp.APP.Integrations.Helpers;
using ToErp.APP.Models;
using ToErp.APP.Models.ConfigModel;
using ToErp.APP.Models.OdooModel;

namespace ToErp.APP.Integrations.OdooErp
{
    public class OdooErpIntegration : IErpIntegration
    {
        #region fields

        private readonly RpcConnection _rpcConnection;
        private int _logoCompanyId = OdooFieldHelper.DefaultLogoCompany;
        private bool _disposed;
        private ILogger<OdooErpIntegration> _logger;

        #endregion

        #region ctors
        public OdooErpIntegration(IOptions<ErpConfig> options, ILogger<OdooErpIntegration> logger, IMapper mapper)
        {
            _logger = logger;
            var connectionSettings = mapper.Map<RpcConnectionSetting>(options.Value);

            _rpcConnection = new RpcConnection(connectionSettings);
        }
        #endregion

        #region implematations
        public void SetOrderToErp<TEntity>(TEntity entity)
        {
            var order = entity as Order;

            WriteSaleOrder(order);
            var orderId = order.OrderID.ToString();

            var shipment = GetShipmentOrder(orderId);
            if (shipment != null)
            {
                shipment.SetFieldValue(OdooFieldHelper.ShipmentPartnerId, GetShipmentProviderId(OdooFieldHelper.YurticiKargo));
                shipment.SetFieldValue(OdooFieldHelper.ShipmentCode, orderId);
                shipment.Save();
            }
        }
        public int GetShipmentProviderId(string name)
        {
            var rpcContext = new RpcContext(_rpcConnection, OdooFieldHelper.ResPartner);

            rpcContext
                .RpcFilter.Equal(OdooFieldHelper.Name, name);

            rpcContext
                .AddField(OdooFieldHelper.Id)
                .AddField(OdooFieldHelper.Name);

            var data = rpcContext.Execute(limit: 1);
            return data.Count() < 0 ? 0 : data.FirstOrDefault().Id;
        }
        public void ChangeOrderStatus<TModel, TStatus>(TModel order, TStatus status)
        {
            var commerceOrder = order as Order;

            OdooOrderStatus orderStatus = ParseOdooOrderStatusToEnum(status);

            var orderId = commerceOrder.OrderID.ToString();

            var saleOrder = GetOrderFromErp<RpcRecord>(orderId);
            if (saleOrder != null)
            {
                saleOrder.SetFieldValue(OdooFieldHelper.LogoErpCompanyId, _logoCompanyId);
                saleOrder.SetFieldValue(OdooFieldHelper.State, orderStatus.ToString());
                saleOrder.Save();
            }

        }
        public void ChangeShipmentStatus<TModel, TStatus>(TModel order, TStatus status)
        {
            var commerceOrder = order as Order;

            if (!string.IsNullOrEmpty(commerceOrder?.ShipperNote))
            {
                var orderId = commerceOrder.OrderID.ToString();

                OrderStatus orderStatus = ParseStatusToEnum(status);

                var shipment = GetShipmentOrder(orderId);

                if (shipment != null)
                {

                    shipment.SetFieldValue(OdooFieldHelper.ShipmentTrackingCode, commerceOrder.ShipperNote);
                    ChangeShipmentStatusToErp(shipment, orderStatus);
                    shipment.Save();

                }
            }
            else
            {
                throw new Exception("ShipperNote is not empty");
            }

        }
        public T GetOrderFromErp<T>(string orderId) where T : class
        {
            return GetSaleOrder(orderId) as T;

        }

        #endregion

        #region local methods
        private RpcRecord WriteSaleOrder(Order order)
        {
            //Partner
            var partner = CreatePartner(order);

            var whichOrder = GetOrderState(order.OrderPayments).ToString();

            var orderLine = GetSaleOrderLine(order);

            RpcRecord record = new RpcRecord(_rpcConnection, OdooFieldHelper.SaleOrder, -1, new List<RpcField>
            {
                new RpcField{FieldName = OdooFieldHelper.LogoErpCompanyId, Value = _logoCompanyId },
                new RpcField{FieldName = OdooFieldHelper.UserId, Value = 56},
                new RpcField{FieldName = OdooFieldHelper.CurrencyId, Value = 31},
                new RpcField{FieldName = OdooFieldHelper.DateOrder, Value = order.OrderDate.ToString(OdooFieldHelper.DateFormat)},
                new RpcField{FieldName = OdooFieldHelper.Name, Value = order.OrderID.ToString()},
                new RpcField{FieldName = OdooFieldHelper.PartnerId, Value = partner.Id},
                new RpcField{FieldName = OdooFieldHelper.PartnerInvoiceId, Value = partner.Id},
                new RpcField{FieldName = OdooFieldHelper.PartnerShippingId, Value = partner.Id},
                new RpcField{FieldName = OdooFieldHelper.PickingPolicy, Value = "one"},
                new RpcField{FieldName = OdooFieldHelper.PricelistId, Value = 1},
                new RpcField{FieldName = OdooFieldHelper.WebsiteId, Value = 1},
                new RpcField{FieldName = OdooFieldHelper.State, Value = whichOrder}, //Onaylı Sipariş ise
                new RpcField{FieldName = OdooFieldHelper.OrderLine, Value =  orderLine.ToArray() }
            });
            record.Save();
            return record;
        }
        private OdooOrderStatus GetOrderState(OrderPayment[] orderPayments)
        {

            foreach (var item in orderPayments)
            {
                if (item.PaymentTypeDesc.Contains(OdooFieldHelper.PayAtTheDoor, StringComparison.OrdinalIgnoreCase)
                    || item.PaymentTypeDesc.Contains(OdooFieldHelper.Transfer, StringComparison.OrdinalIgnoreCase))
                {
                    return OdooOrderStatus.draft;
                }

            }

            return OdooOrderStatus.sale;
        }
        private List<object> GetSaleOrderLine(Order order)
        {
            var orderLine = new List<object>();
            foreach (var line in order.OrderDetails)
            {
                var product = GetSearchProductByDefaultCode(line.SKU);
                if (product == null)
                {
                    _logger.LogWarning($"Odoo'da Commerce'den gelen SKU({line.SKU})'lu urun bulunamadı");
                    continue;
                }

                List<RpcRecord> bundleLine = new List<RpcRecord>();

                var bundleProduct = ProcessIfBundleProduct(product, bundleLine, order.OrderSubTotal);


                if (bundleLine.Count > 0)
                {
                    var objectBundle = bundleLine.Select(x => new object[] { 0, 0, x.GetRecord() });
                    orderLine.AddRange(objectBundle);
                    continue;
                }

                RpcRecord record = CreateOrderLine(
                    product.Id,
                    product.GetField(OdooFieldHelper.Name).Value,
                    line.LineTotal, line.Quantity,
                    product.GetField(OdooFieldHelper.TaxesId).Value);


                orderLine.Add(new object[] { 0, 0, record.GetRecord() });
            }

            return orderLine;
        }
        private RpcRecord CreatePartner(Order order)
        {
            var stateId = GetCountryStateByName(order.ShipCity);   
            
            var countryId = GetCountryId(OdooFieldHelper.TR);

            RpcRecord partner = new RpcRecord(_rpcConnection, OdooFieldHelper.ResPartner, -1, new List<RpcField>
            {
                new RpcField{FieldName = OdooFieldHelper.Name, Value = $"{order.ShipName} {order.ShipLastName}"},
                new RpcField{FieldName = OdooFieldHelper.Email, Value = order.Email},
                new RpcField{FieldName = OdooFieldHelper.Street, Value = order.ShipTown},
                new RpcField{FieldName = OdooFieldHelper.Street2, Value = order.ShipAddress},
                new RpcField{FieldName = OdooFieldHelper.City, Value = order.ShipCity},
                new RpcField{FieldName = OdooFieldHelper.StateId, Value = stateId},
                new RpcField{FieldName = OdooFieldHelper.CountryId, Value = countryId },
                new RpcField{FieldName = OdooFieldHelper.Vat, Value = String.IsNullOrEmpty(order.TaxNumber) ? OdooFieldHelper.DefaultVat : order.TaxNumber },
              
            });

            partner.Save();

            return partner;

        }
        public int GetCountryId(string countryCode)
        {
            var rpcContext = new RpcContext(_rpcConnection, OdooFieldHelper.ResCountry);
            
            rpcContext
                .RpcFilter
                .Equal(OdooFieldHelper.Code, countryCode);
            
            rpcContext.AddField(OdooFieldHelper.Id);

            var data = rpcContext.Execute(limit: 1);
            
            var country = data.FirstOrDefault().Id;
            
            return country;
        }
        private int GetCountryStateByName(string stateName)
        {
            stateName = CapitalizeName(stateName);

            var rpcContext = new RpcContext(_rpcConnection, OdooFieldHelper.ResCountryState);

            rpcContext
                .RpcFilter.Equal(OdooFieldHelper.Name, stateName);

            rpcContext
                .AddField(OdooFieldHelper.Id);

            var data = rpcContext.Execute(limit: 1);
            return data.FirstOrDefault().Id;

        }
        private RpcRecord GetSearchProductByDefaultCode(string defaultCode)
        {
            var rpcContext = new RpcContext(_rpcConnection, OdooFieldHelper.ProductProduct);

            rpcContext
                .RpcFilter
                .Equal(OdooFieldHelper.DefaultCode, defaultCode);

            rpcContext
                .AddField(OdooFieldHelper.Id)
                .AddField(OdooFieldHelper.Name)
                .AddField(OdooFieldHelper.TaxesId)
                .AddField(OdooFieldHelper.ListPrice)
                .AddField(OdooFieldHelper.BomCount)
                .AddField(OdooFieldHelper.ProductTmplId);

            var data = rpcContext.Execute(true, limit: 1);
            return data.FirstOrDefault();
        }
        private RpcRecord GetProductById(int id)
        {
            var rpcContext = new RpcContext(_rpcConnection, OdooFieldHelper.ProductProduct);

            rpcContext
                .RpcFilter
                .Equal(OdooFieldHelper.Id, id);

            rpcContext
                .AddField(OdooFieldHelper.Id)
                .AddField(OdooFieldHelper.Name)
                .AddField(OdooFieldHelper.TaxesId)
                .AddField(OdooFieldHelper.ListPrice)
                .AddField(OdooFieldHelper.BomCount)
                .AddField(OdooFieldHelper.ProductTmplId);

            var data = rpcContext.Execute(true, limit: 1);
            return data.FirstOrDefault();
        }
        private RpcRecord GetSaleOrder(string orderId)
        {
            var rpcContext = new RpcContext(_rpcConnection, OdooFieldHelper.SaleOrder);

            rpcContext.RpcFilter.Equal(OdooFieldHelper.Name, orderId);

            rpcContext.AddField(OdooFieldHelper.Id)
                .AddField(OdooFieldHelper.AmountTotal)
                .AddField(OdooFieldHelper.XPriceCheck)
                .AddField(OdooFieldHelper.State);

            var data = rpcContext.Execute(true, limit: 1);
            return data.FirstOrDefault();

        }
        private void ChangeShipmentStatusToErp(RpcRecord shipment, OrderStatus orderStatus = OrderStatus.Gönderildi)
        {
            var curiousField = OdooFieldHelper.XShipmentStatus;

            var status = shipment.GetField(curiousField).Value?.ToString();
            if (status != null)
            {
                if (status.Equals(orderStatus))
                {
                    return;
                }

            }
            shipment.SetFieldValue(curiousField, orderStatus.ToString());
        }
        private RpcRecord GetShipmentOrder(string orderId)
        {
            var rpcContext = new RpcContext(_rpcConnection, OdooFieldHelper.StockPicking);

            rpcContext.RpcFilter.Equal(OdooFieldHelper.Origin, orderId);

            rpcContext.AddField(OdooFieldHelper.Id)
                .AddField(OdooFieldHelper.ShipmentCode)
                .AddField(OdooFieldHelper.ShipmentPartnerId)
                .AddField(OdooFieldHelper.ShipmentTrackingCode)
                .AddField(OdooFieldHelper.XShipmentStatus);

            var data = rpcContext.Execute(true, limit: 1);
            return data.FirstOrDefault();

        }
        private OrderStatus ParseStatusToEnum(object status)
        {
            OrderStatus orderStatus;
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
        private OdooOrderStatus ParseOdooOrderStatusToEnum(object status)
        {
            OdooOrderStatus orderStatus;
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
        private RpcRecord CreateOrderLine(int productId, object name, decimal? lineTotal, int lineQuantity, object taxes_id)
        {
            return new RpcRecord(_rpcConnection, OdooFieldHelper.SaleOrderLine, -1, new List<RpcField>
                            {
                                new RpcField{FieldName = OdooFieldHelper.Name, Value = name},
                                new RpcField{FieldName = OdooFieldHelper.PriceUnit, Value = lineTotal.HasValue ? lineTotal.Value.ToString() : "0"},
                                new RpcField{FieldName = OdooFieldHelper.ProductUomQty, Value = lineQuantity},
                                new RpcField{FieldName = OdooFieldHelper.ProductId, Value = productId},
                                new RpcField{FieldName = OdooFieldHelper.TaxId, Value = taxes_id},
                            });
        }
        private decimal? GetOdooPriceFromProduct(RpcRecord product)
        {
            decimal? price = null;
            try
            {
                price = Convert.ToDecimal(product.GetField(OdooFieldHelper.ListPrice).Value);

            }
            catch (Exception exception)
            {
                _logger.LogError($"{product.GetField(OdooFieldHelper.Name).Value} an error occurred during convert , exception : {exception.Message}");
            }

            return price;
        }
        private List<RpcRecord> ProcessIfBundleProduct(RpcRecord rpcRecord, List<RpcRecord> bundleLine, decimal orderSubTotal)
        {
            if (rpcRecord != null)
            {
                var isHasBundleProduct = IsBundleProduct(rpcRecord);

                if (!isHasBundleProduct)
                {
                    return null;
                }
                var mrpBomCompenent = GetMrpBom();

                var productProductTemplate = RpcRecordResponseModel.RpcRecordParseToResponseModel(rpcRecord, OdooFieldHelper.ProductTmplId);

                foreach (var item in mrpBomCompenent)
                {
                    var mrpBomproductTemplate = RpcRecordResponseModel.RpcRecordParseToResponseModel(item, OdooFieldHelper.ProductTmplId);

                    if (productProductTemplate.Id == mrpBomproductTemplate.Id)
                    {
                        var mrpBomLineCompenent = GetMrpBomLine();

                        foreach (var mrpBomLineId in mrpBomLineCompenent)
                        {
                            var mrpBomLineBomId = RpcRecordResponseModel.RpcRecordParseToResponseModel(mrpBomLineId, OdooFieldHelper.BomId);

                            if (mrpBomLineBomId.Id == item.Id)
                            {
                                var productId = RpcRecordResponseModel.RpcRecordParseToResponseModel(mrpBomLineId, OdooFieldHelper.ProductId);

                                var product = GetProductById(productId.Id);

                                decimal price = (Convert.ToDecimal(mrpBomLineId.GetField(OdooFieldHelper.AmountPercent).Value) * orderSubTotal) / 100;

                                if (IsBundleProduct(product))
                                {
                                    return ProcessIfBundleProduct(product, bundleLine, price);
                                }

                                bundleLine.Add(CreateOrderLine(product.Id,
                                        product.GetField(OdooFieldHelper.Name).Value,
                                        price, Convert.ToInt32(mrpBomLineId.GetField(OdooFieldHelper.ProductQty).Value),
                                        product.GetField(OdooFieldHelper.TaxesId).Value));
                            }

                        }

                    }
                }
            }
            return bundleLine;

        }
        private bool IsBundleProduct(RpcRecord product)
        {
            if (product != null)
            {
                int bomCount = 0;

                var wasParsed = int.TryParse(product.GetField(OdooFieldHelper.BomCount).Value.ToString(), out bomCount);

                if (wasParsed)
                {
                    if (bomCount > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        private IEnumerable<RpcRecord> GetMrpBom()
        {
            var mrpBom = new RpcContext(_rpcConnection, OdooFieldHelper.MrpBom);

            mrpBom
                .AddField(OdooFieldHelper.Id)
                .AddField(OdooFieldHelper.ProductQty)
                .AddField(OdooFieldHelper.ProductTmplId);

            return mrpBom.Execute(true, limit: 100);
        }
        private IEnumerable<RpcRecord> GetMrpBomLine()
        {
            var mrpBomLine = new RpcContext(_rpcConnection, OdooFieldHelper.MrpBomLine);

            mrpBomLine
                .AddField(OdooFieldHelper.Id)
                .AddField(OdooFieldHelper.ProductQty)
                .AddField(OdooFieldHelper.ProductTmplId)
                .AddField(OdooFieldHelper.BomId)
                .AddField(OdooFieldHelper.ProductId)
                .AddField(OdooFieldHelper.AmountPercent);

            return mrpBomLine.Execute(true, limit: 100);
        }
        private string CapitalizeName(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            string newName = string.Empty;
            for (int i = 0; i < name.Length; i++)
            {
                if (i == 0)
                {
                    newName += char.ToUpper(name[i], new CultureInfo("tr-TR", false));
                    continue;
                }
                newName += char.ToLower(name[i], new CultureInfo("tr-TR", false));

            }
            return newName;
        }
        private int GetTaxId(string invoiceNumber)
        {
            if (String.IsNullOrEmpty(invoiceNumber))
            {
                return default(int);
            }
            var rpcContext = new RpcContext(_rpcConnection, OdooFieldHelper.AccountTaxOffice);

            rpcContext.RpcFilter
                .Equal(OdooFieldHelper.Code, invoiceNumber);


            rpcContext.AddField(OdooFieldHelper.Id);

            var data = rpcContext.Execute(limit: 1);

            var vergiDairesi = data.FirstOrDefault().Id;

            return vergiDairesi;

        }

        #endregion

        #region dispose
        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                GC.SuppressFinalize(_rpcConnection);
            }

            _disposed = true;
        }

        #endregion
    }

    public class RpcRecordResponseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }


        public static RpcRecordResponseModel RpcRecordParseToResponseModel(RpcRecord rpcRecord, string fieldName)
        {
            var arrayModel = (object[])rpcRecord.GetField(fieldName)?.Value;

            if (arrayModel == null)
            {
                return null;
            }

            if (arrayModel.Length < 2)
            {
                return null;
            }

            int productTemplateId = 0;
            var wasParsed = int.TryParse(arrayModel[0].ToString(), out productTemplateId);

            if (!wasParsed)
            {
                return null;
            }

            var productTemplateName = arrayModel[1].ToString();

            return new RpcRecordResponseModel
            {
                Id = productTemplateId,
                Name = productTemplateName
            };
        }
    }
}