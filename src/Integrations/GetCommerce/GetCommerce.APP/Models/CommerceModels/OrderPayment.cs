using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetCommerce.APP.Models.CommerceModels
{
    public class OrderPayment
    {
        public string AccountNumber { get; set; }

        public string CanceledDescription { get; set; }

        public System.Nullable<int> CardID { get; set; }

        public string CreatedBy { get; set; }

        public System.DateTime CreatedDate { get; set; }

        public string Description { get; set; }

        public int ID { get; set; }

        public System.Nullable<int> InstallmentCount { get; set; }

        public System.Nullable<decimal> InterestCost { get; set; }

        public decimal LocalExchangeRate { get; set; }

        public string ModifiedBy { get; set; }

        public System.DateTime ModifiedDate { get; set; }

        public string MoneyOrderAccountID { get; set; }

        public System.Nullable<int> OrderID { get; set; }

        public string PaymentCurrency { get; set; }

        public int PaymentID { get; set; }

        public string PaymentState { get; set; }

        public System.Nullable<int> PaymentTransactionID { get; set; }

        public string PaymentTypeDesc { get; set; }

        public System.Nullable<decimal> TotalAmount { get; set; }

        public System.Nullable<int> UserID { get; set; }
    }
}
