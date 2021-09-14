using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetCommerce.APP.Models.CommerceModels
{
    public class Order
    {
        public string AccountNumber { get; set; }

        public string BillAddress { get; set; }

        public System.Nullable<int> BillAddressID { get; set; }

        public string BillCity { get; set; }

        public string BillCityCode { get; set; }

        public string BillCountry { get; set; }

        public string BillCountryCode { get; set; }

        public string BillDistrict { get; set; }

        public string BillDistrictCode { get; set; }

        public string BillLastName { get; set; }

        public string BillMobilePhone { get; set; }

        public string BillName { get; set; }

        public string BillPhone { get; set; }

        public string BillPostalCode { get; set; }

        public string BillTown { get; set; }

        public string CompanyName { get; set; }

        public string Email { get; set; }

        public decimal InterestCost { get; set; }

        public string InvoiceCode { get; set; }

        public string LastName { get; set; }

        public decimal LocalExchangeRate { get; set; }

        public string Name { get; set; }

        public string OrderCode { get; set; }

        public System.DateTime OrderDate { get; set; }

        public OrderDetail[] OrderDetails { get; set; }

        public OrderDiscount[] OrderDiscounts { get; set; }

        public int OrderID { get; set; }

        public OrderPayment[] OrderPayments { get; set; }

        public string OrderSource { get; set; }

        public string OrderSourceBarcode { get; set; }

        public int OrderState { get; set; }

        public string OrderStateName { get; set; }

        public decimal OrderSubTotal { get; set; }

        public OrderSurcharge[] OrderSurcharges { get; set; }

        public string PaymentCurrency { get; set; }

        public string SalesContract { get; set; }

        public string SellerNote { get; set; }

        public string ShipAddress { get; set; }

        public System.Nullable<int> ShipAddressID { get; set; }

        public string ShipCity { get; set; }

        public string ShipCityCode { get; set; }

        public string ShipCountry { get; set; }

        public string ShipCountryCode { get; set; }

        public string ShipDistrict { get; set; }

        public string ShipDistrictCode { get; set; }

        public string ShipFullNameOther { get; set; }

        public string ShipGiftNote { get; set; }

        public string ShipLastName { get; set; }

        public string ShipMobilePhone { get; set; }

        public string ShipName { get; set; }

        public string ShipPhone { get; set; }

        public string ShipPostalCode { get; set; }

        public string ShipReferenceCode { get; set; }

        public string ShipTown { get; set; }

        public string ShipperID { get; set; }

        public string ShipperNote { get; set; }

        public decimal ShippingCost { get; set; }

        public string TaxNumber { get; set; }

        public string TaxOffice { get; set; }

        public string UserID { get; set; }

        public string UserNote { get; set; }


    }
}
