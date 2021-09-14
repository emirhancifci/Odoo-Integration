using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToErp.APP.Models
{
    public class VariantPropertyInfo
    {
        public int DefinitionID { get; set;}

        public string DefinitionName { get; set;}

        public VariantPropertyItem[] VariantProperties { get; set; }
    }
}
