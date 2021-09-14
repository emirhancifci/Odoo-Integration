using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Odoo.Concrete;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ToErp.APP.Integrations.Abstractions;
using ToErp.APP.Models;
using ToErp.APP.Models.ConfigModel;
using ToErp.APP.Models.OdooModel;

namespace ToErp.APP.Integrations.OdooErp
{
    public class OdooErpIntegration : IErpIntegration
    {
        #region fields

        private readonly RpcConnection _rpcConnection;
        private int _logoCompanyId = 2;
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
        }
        public void ChangeOrderStatus<TStatus>(string orderId, TStatus status)
        {
            OdooOrderStatus orderStatus = ParseOdooOrderStatusToEnum(status);
            var saleOrder = GetOrderFromErp<RpcRecord>(orderId);
            saleOrder.SetFieldValue("state", orderStatus.ToString());
            saleOrder.Save();

        }
        public void ChangeShipmentStatus<TStatus>(string orderId, TStatus status)
        {
            OrderStatus orderStatus = ParseStatusToEnum(status);
            ChangeShipmentStatusToErp(orderId, orderStatus);
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

            RpcRecord record = new RpcRecord(_rpcConnection, "sale.order", -1, new List<RpcField>
            {

                //new RpcField{FieldName = "client_order_ref", Value = "REF-1"},
               // new RpcField{FieldName = "company_id", Value = 1 },
                new RpcField{FieldName = "logo_erp_company_id", Value = _logoCompanyId },
                new RpcField{FieldName = "user_id", Value = 56},
                new RpcField{FieldName = "currency_id", Value = 31},
                new RpcField{FieldName = "date_order", Value = order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss")},
                new RpcField{FieldName = "name", Value = order.OrderID.ToString()},
                new RpcField{FieldName = "partner_id", Value = partner.Id},
                new RpcField{FieldName = "partner_invoice_id", Value = partner.Id},
                new RpcField{FieldName = "partner_shipping_id", Value = partner.Id},
                new RpcField{FieldName = "picking_policy", Value = "one"},
                new RpcField{FieldName = "pricelist_id", Value = 1},
                //new RpcField{FieldName = "warehouse_id", Value = 5},
                new RpcField{FieldName = "website_id", Value = 1},
                new RpcField{FieldName = "state", Value = whichOrder}, //Onaylı Sipariş ise
                new RpcField{FieldName = "order_line", Value =  orderLine.ToArray() }
            });
            record.Save();
            return record;
        }

        private OdooOrderStatus GetOrderState(OrderPayment[] orderPayments)
        {

            foreach (var item in orderPayments)
            {
                if (item.PaymentTypeDesc.Contains("Kapıda Ödeme",StringComparison.OrdinalIgnoreCase) || item.PaymentTypeDesc.Contains("Havale", StringComparison.OrdinalIgnoreCase))
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
                var product = GetSearchProductByDefaultCode(line.SKU); //"KK000177");
                if (product == null)
                {
                    _logger.LogWarning($"Odoo'da Commerce'den gelen SKU({line.SKU})'lu urun bulunamadı");
                    continue;
                }
                List<RpcRecord> bundleLine = new List<RpcRecord>();

                var bundleProduct = ProcessIfBundleProduct(product, bundleLine);


                if (bundleLine.Count > 0)
                {
                    var objectBundle = bundleLine.Select(x => new object[] { 0, 0, x.GetRecord() });
                    orderLine.AddRange(objectBundle);
                    continue;
                }

                //decimal? odooPrice = GetOdooPriceFromProduct(product);
                RpcRecord record = CreateOrderLine(
                    product.Id,
                    product.GetField("name").Value,
                    Convert.ToInt64(line.LineTotal), line.Quantity,
                    product.GetField("taxes_id").Value);


                orderLine.Add(new object[] { 0, 0, record.GetRecord() });
            }

            return orderLine;
        }
        private RpcRecord CreatePartner(Order order)
        {
            //TODO : IF check already exist partner 
            var stateId = GetCountryStateByName(order.ShipCity);
            //var taxId = GetTaxId(order.TaxOffice);
            var countryId = GetCountryId("TR");
            RpcRecord partner = new RpcRecord(_rpcConnection, "res.partner", -1, new List<RpcField>
            {
                new RpcField{FieldName = "name", Value = $"{order.ShipName} {order.ShipLastName}"},
                new RpcField{FieldName = "email", Value = order.Email},
                //new RpcField{FieldName = "phone", Value = order.ShipMobilePhone},
                new RpcField{FieldName = "street", Value = order.ShipTown},
                new RpcField{FieldName = "street2", Value = order.ShipAddress},
                new RpcField{FieldName = "city", Value = order.ShipCity},
                new RpcField{FieldName = "state_id", Value = stateId},
                new RpcField{FieldName = "country_id", Value = countryId },
                new RpcField{FieldName = "vat", Value = String.IsNullOrEmpty(order.TaxNumber) ? "11111111111" : order.TaxNumber },
                //new RpcField{FieldName = "parent_id", Value = false },
            });

            partner.Save();

            return partner;

        }
        public int GetCountryId(string countryCode)
        {
            var rpcContext = new RpcContext(_rpcConnection, "res.country");
            rpcContext.RpcFilter.Equal("code", countryCode);
            rpcContext.AddField("id");
            var data = rpcContext.Execute(limit: 1);
            var ulke = data.FirstOrDefault().Id;
            return ulke;
        }
        private int GetCountryStateByName(string stateName)
        {
            stateName = CapitalizeName(stateName);

            var rpcContext = new RpcContext(_rpcConnection, "res.country.state");

            rpcContext
                .RpcFilter.Equal("name", stateName);

            rpcContext
                .AddField("id");

            var data = rpcContext.Execute(limit: 1);
            return data.FirstOrDefault().Id;

        }
        private RpcRecord GetSearchProductByDefaultCode(string defaultCode)
        {
            var rpcContext = new RpcContext(_rpcConnection, "product.product");

            rpcContext
                .RpcFilter
                .Equal("default_code", defaultCode);

            rpcContext
                .AddField("id")
                .AddField("name")
                .AddField("taxes_id")
                .AddField("list_price")
                .AddField("bom_count")
                .AddField("product_tmpl_id");

            var data = rpcContext.Execute(true, limit: 1);
            return data.FirstOrDefault();
        }
        private RpcRecord GetProductById(int id)
        {
            var rpcContext = new RpcContext(_rpcConnection, "product.product");

            rpcContext
                .RpcFilter
                .Equal("id", id);

            rpcContext
                .AddField("id")
                .AddField("name")
                .AddField("taxes_id")
                .AddField("list_price")
                .AddField("bom_count")
                .AddField("product_tmpl_id");

            var data = rpcContext.Execute(true, limit: 1);
            return data.FirstOrDefault();
        }
        private RpcRecord GetSaleOrder(string orderId)
        {
            var rpcContext = new RpcContext(_rpcConnection, "sale.order");

            rpcContext.RpcFilter.Equal("name", orderId);

            rpcContext.AddField("id")
                .AddField("amount_total")
                .AddField("x_price_check")
                .AddField("state");

            var data = rpcContext.Execute(true, limit: 1);
            return data.FirstOrDefault();

        }
        private void ChangeShipmentStatusToErp(string orderId, OrderStatus orderStatus = OrderStatus.Gönderildi)
        {
            var curiousField = "x_shipment_status";
            var shipmentOrder = GetShipmentOrder(orderId);
            var status = shipmentOrder.GetField(curiousField).Value?.ToString();
            if (status != null)
            {
                if (status.Equals(orderStatus))
                {
                    return;
                }

            }
            shipmentOrder.SetFieldValue(curiousField, orderStatus.ToString());
            shipmentOrder.Save();

        }
        private RpcRecord GetShipmentOrder(string orderId)
        {
            var rpcContext = new RpcContext(_rpcConnection, "stock.picking");

            rpcContext.RpcFilter.Equal("origin", orderId);

            rpcContext.AddField("id")
                .AddField("x_shipment_status");

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
            return new RpcRecord(_rpcConnection, "sale.order.line", -1, new List<RpcField>
                            {
                                new RpcField{FieldName = "name", Value = name},
                                new RpcField{FieldName = "price_unit", Value = Convert.ToInt64(lineTotal)},
                                new RpcField{FieldName = "product_uom_qty", Value = lineQuantity},
                                new RpcField{FieldName = "product_id", Value = productId},
                                new RpcField{FieldName = "tax_id", Value = taxes_id},
                            });
        }
        private decimal? GetOdooPriceFromProduct(RpcRecord product)
        {
            decimal? price = null;
            try
            {
                price = Convert.ToDecimal(product.GetField("list_price").Value);

            }
            catch (Exception exception)
            {
                _logger.LogError($"{product.GetField("name").Value} an error occurred during convert , exception : {exception.Message}");
            }

            return price;
        }
        private List<RpcRecord> ProcessIfBundleProduct(RpcRecord rpcRecord, List<RpcRecord> bundleLine)
        {
            if (rpcRecord != null)
            {
                var isHasBundleProduct = IsBundleProduct(rpcRecord);

                if (!isHasBundleProduct)
                {
                    return null;
                }
                var mrpBomCompenent = GetMrpBom();

                var productProductTemplate = RpcRecordResponseModel.RpcRecordParseToResponseModel(rpcRecord, "product_tmpl_id");

                foreach (var item in mrpBomCompenent)
                {
                    var mrpBomproductTemplate = RpcRecordResponseModel.RpcRecordParseToResponseModel(item, "product_tmpl_id");

                    if (productProductTemplate.Id == mrpBomproductTemplate.Id)
                    {
                        var mrpBomLineCompenent = GetMrpBomLine();

                        foreach (var mrpBomLineId in mrpBomLineCompenent)
                        {
                            var mrpBomLineBomId = RpcRecordResponseModel.RpcRecordParseToResponseModel(mrpBomLineId, "bom_id");

                            if (mrpBomLineBomId.Id == item.Id)
                            {
                                var productId = RpcRecordResponseModel.RpcRecordParseToResponseModel(mrpBomLineId, "product_id");

                                var product = GetProductById(productId.Id);

                                if (IsBundleProduct(product))
                                {
                                    return ProcessIfBundleProduct(product, bundleLine);
                                }

                                decimal? price = (Convert.ToDecimal(mrpBomLineId.GetField("amount_percent").Value) * Convert.ToDecimal(rpcRecord.GetField("list_price").Value)) / 100;

                                bundleLine.Add(CreateOrderLine(product.Id,
                                        product.GetField("name").Value,
                                        Convert.ToInt64(price), Convert.ToInt32(mrpBomLineId.GetField("product_qty").Value),
                                        product.GetField("taxes_id").Value));
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

                var wasParsed = int.TryParse(product.GetField("bom_count").Value.ToString(), out bomCount);

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
            var mrpBom = new RpcContext(_rpcConnection, "mrp.bom");

            mrpBom
                .AddField("id")
                .AddField("product_qty")
                .AddField("product_tmpl_id");

            return mrpBom.Execute(true, limit: 100);
        }
        private IEnumerable<RpcRecord> GetMrpBomLine()
        {
            var mrpBomLine = new RpcContext(_rpcConnection, "mrp.bom.line");

            mrpBomLine
                .AddField("id")
                .AddField("product_qty")
                .AddField("product_tmpl_id")
                .AddField("bom_id")
                .AddField("product_id")
                .AddField("amount_percent");

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
            var rpcContext = new RpcContext(_rpcConnection, "account.tax.office"); rpcContext.RpcFilter
                .Equal("code", invoiceNumber);
            rpcContext.AddField("id");
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