using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToErp.APP.Models
{
    public class OrderDetail
    {
        public string Currency { get; set; }

        public decimal Discount { get; set; }

        public string DiscountDesc { get; set; }

        public string ItemName { get; set; }

        public string ItemNote { get; set; }

        public OrderDetailProperty[] ItemProperties { get; set; }

        public decimal LineLocalExchangeRate { get; set; }

        public string LineStatus { get; set; }

        public System.Nullable<decimal> LineTotal { get; set; }

        public string LineType { get; set; }

        public int OrderDetailID { get; set; }

        public int OrderID { get; set; }

        public int Quantity { get; set; }

        public string SKU { get; set; }

        public string ShipperID { get; set; }

        public System.DateTime ShippingDate { get; set; }

        public string ShippingRefCode { get; set; }

        public string ShippingStatus { get; set; }

        public decimal TaxRate { get; set; }

        public decimal UnitCost { get; set; }

        public VariantPropertyInfo VariantInfo { get; set; }

        public string VariantSKU { get; set; }

        public int WareHouse { get; set; }
    }
}
