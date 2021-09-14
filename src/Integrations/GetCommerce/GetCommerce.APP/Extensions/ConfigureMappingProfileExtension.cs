using AutoMapper;
using GetCommerce.APP.Mappings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetCommerce.APP.Extensions
{
    public static class ConfigureMappingProfileExtension
    {
        public static IServiceCollection ConfigureMapping(this IServiceCollection services)
        {
            var types = typeof(AutoMapperBaseProfile).Assembly.GetTypes();

            var autoMapperProfiles = types
                .Where(x => x.IsSubclassOf(typeof(AutoMapperBaseProfile)))
                .Select(Activator.CreateInstance)
                .OfType<AutoMapperBaseProfile>().ToList();

            var config = new MapperConfiguration(i =>
            {
                i.AddProfiles(autoMapperProfiles);
            });
            IMapper mapper = config.CreateMapper();
            services.AddSingleton(mapper);
            return services;

        }
    }
}
