using AutoMapper;
using Odoo.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToErp.APP.Models.ConfigModel;

namespace ToErp.APP.Mappings
{
    public class AutoMapperConfigMappingProfile : AutoMapperBaseProfile
    {
        public AutoMapperConfigMappingProfile()
        {
            CreateMap<RpcConnectionSetting, ErpConfig>()
                .ReverseMap();
        }
    }
}
