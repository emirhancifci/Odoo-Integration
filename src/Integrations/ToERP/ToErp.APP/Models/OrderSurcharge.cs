using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToErp.APP.Models
{
    public class OrderSurcharge
    {
        public int OrderID { get; set; }

        public decimal SurchargeAmount { get; set; }

        public int SurchargeID { get; set; }

        public string SurchargeName { get; set; }

        public decimal VatRate { get; set; }
    }
}
