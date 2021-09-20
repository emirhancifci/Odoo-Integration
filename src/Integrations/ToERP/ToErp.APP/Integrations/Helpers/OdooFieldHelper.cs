using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToErp.APP.Integrations.Helpers
{
    public class OdooFieldHelper
    {
        // Is Order Draft ?
        public const string PayAtTheDoor = "Kapıda Ödeme";
        public const string Transfer = "Havale";


        // Date Format
        public const string DateFormat = "yyyy-MM-dd HH:mm:ss";


        // Rpc Models
        public const string SaleOrder = "sale.order";
        public const string SaleOrderLine = "sale.order.line";
        public const string AccountTaxOffice = "account.tax.office";
        public const string MrpBom = "mrp.bom";
        public const string MrpBomLine = "mrp.bom.line";
        public const string StockPicking = "stock.picking";
        public const string ProductProduct = "product.product";

        public const string ResCountry = "res.country";
        public const string ResCountryState = "res.country.state";
        public const string ResPartner = "res.partner";



        // Global
        public const string Id = "id";
        public const string Name = "name";
        public const string Code = "code";
        public const string DefaultVat = "11111111111";
        public const int DefaultLogoCompany = 2;



        // Shipment
        public const string ShipmentPartnerId = "shipment_partner_id";
        public const string ShipmentCode = "shipment_code";
        public const string ShipmentTrackingCode = "shipment_tracking_code";
        public const string XShipmentStatus = "x_shipment_status";
        public const string XPriceCheck = "x_price_check";
        public const string LogoErpCompanyId = "logo_erp_company_id";
        
        
        

        // necessary for create order
        public const string UserId = "user_id";
        public const string CurrencyId = "currency_id";
        public const string DateOrder = "date_order";
        public const string PartnerId = "partner_id";
        public const string PartnerInvoiceId = "partner_invoice_id";
        public const string PartnerShippingId = "partner_shipping_id";
        public const string PickingPolicy = "picking_policy";
        public const string PricelistId = "pricelist_id";
        public const string WebsiteId = "website_id";
        public const string State = "state";
        public const string OrderLine = "order_line";
        public const string TaxesId = "taxes_id";



        // Bundle Product - Product  Field
        public const string BomId = "bom_id";
        public const string ProductQty = "product_qty";
        public const string ProductTmplId = "product_tmpl_id";
        public const string ProductId = "product_id";
        public const string AmountPercent = "amount_percent";
        public const string BomCount = "bom_count";
        public const string ListPrice = "list_price";
        public const string PriceUnit = "price_unit";
        public const string ProductUomQty = "product_uom_qty";
        public const string TaxId = "tax_id";


        public const string Origin = "origin";
        public const string AmountTotal = "amount_total";
        public const string DefaultCode = "default_code";


        public const string Email = "email";
        public const string Street = "street";
        public const string Street2 = "street2";
        public const string City = "city";
        public const string StateId = "state_id";
        public const string CountryId = "country_id";
        public const string Vat = "vat";


        // Cargo Componies
        public const string YurticiKargo = "Yurtiçi Kargo";

        // Country
        public const string TR = "TR";












    }
}
