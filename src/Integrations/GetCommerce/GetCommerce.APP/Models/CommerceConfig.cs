using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetCommerce.APP.Models
{
    public class CommerceConfig
    {
        [JsonProperty("EndPoint")]
        public string EndPoint { get; set; }
        [JsonProperty("UserName")]
        public string UserName { get; set; }
        [JsonProperty("Password")]
        public string Password { get; set; }
    }
}
