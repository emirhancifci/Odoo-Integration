using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetCommerce.APP.Models.CommerceModels
{
    public class OrderDiscount
    {
        public System.Nullable<decimal> DiscountAmount { get; set; }

        public int DiscountID { get; set; }

        public string DiscountName { get; set; }

        public System.Nullable<decimal> DiscountRate { get; set; }

        public bool IsPercentage { get; set; }

        public int OrderID { get; set; }
    }
}
