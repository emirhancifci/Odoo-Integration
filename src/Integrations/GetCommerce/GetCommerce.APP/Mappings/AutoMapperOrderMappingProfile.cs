using AutoMapper;
using GetCommerce.APP.Models.CommerceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetCommerce.APP.Mappings
{
    public class AutoMapperOrderMappingProfile : AutoMapperBaseProfile
    {
        public AutoMapperOrderMappingProfile()
        {
            CreateMap<Kmwsc.Order, Order>()
                .ReverseMap();

            CreateMap<Kmwsc.OrderDetail, OrderDetail>()
                .ReverseMap();

            CreateMap<Kmwsc.OrderDetailProperty, OrderDetailProperty>()
                .ReverseMap();


            CreateMap<Kmwsc.OrderDiscount, OrderDiscount>()
                .ReverseMap();


            CreateMap<Kmwsc.OrderPayment, OrderPayment>()
                .ReverseMap();


            CreateMap<Kmwsc.OrderSurcharge, OrderSurcharge>()
                .ReverseMap();


            CreateMap<Kmwsc.VariantPropertyInfo, VariantPropertyInfo>()
                .ReverseMap();


            CreateMap<Kmwsc.VariantPropertyItem, VariantPropertyItem>()
                .ReverseMap();
        }
    }
}
