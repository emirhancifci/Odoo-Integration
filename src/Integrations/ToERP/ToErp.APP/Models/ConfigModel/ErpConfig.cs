using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToErp.APP.Models.ConfigModel
{
    public class ErpConfig
    {
        public string ServerUrl { get; set; }
        public string DbName { get; set; }
        public string DbUser { get; set; }
        public string DbPassword { get; set; }
        public bool ImmediateLogin { get; set; }
        public bool ServerCertificateValidation { get; set; }
    }
}
