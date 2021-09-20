using EventBus.Base;
using EventBus.Base.Abstractions;
using EventBus.Factory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using ToErp.APP.Extensions;
using ToErp.APP.IntegrationEvents.Events;
using ToErp.APP.IntegrationEvents.Handlers;
using ToErp.APP.Integrations.Abstractions;
using ToErp.APP.Integrations.OdooErp;
using ToErp.APP.Models.ConfigModel;

namespace ToErp.APP
{
    class Program
    {
        #region fields and prop
        public static IConfiguration Configuration { get; set; }
        static readonly AutoResetEvent autoEvent = new(false);

        #endregion
        static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();

            ConfigurationService(services);

            ConfigureServices(services);

            var sp = services.BuildServiceProvider();

            IEventBus eventBus = sp.GetRequiredService<IEventBus>();

            eventBus.Subcribe<GetOrdersIntegrationEvent, GetOrderIntegrationEventHandler>();
            eventBus.Subcribe<GetSentOrdersIntegrationEvent, GetSentOrderIntegrationEventHandler>();
            eventBus.Subcribe<GetCancelledOrdersIntegrationEvent, GetCancelledIntegrationEventHandler>();
            eventBus.Subcribe<GetCompletedOrdersIntegrationEvent, GetCompletedOrdersIntegrationEventHandler>();
            eventBus.Subcribe<CheckPriceIntegrationEvent, CheckPriceIntegrationEventHandler>();

            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);

            autoEvent.WaitOne();

        }

        #region configurations
        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure =>
            {
                configure.AddConsole();
            });

            services.ConfigureMapping();

            services.Configure<ErpConfig>(settings => Configuration.GetSection(nameof(ErpConfig)).Bind(settings));

            services.AddTransient<GetOrderIntegrationEventHandler>();
            services.AddTransient<GetCancelledIntegrationEventHandler>();
            services.AddTransient<GetSentOrderIntegrationEventHandler>();
            services.AddTransient<GetCompletedOrdersIntegrationEventHandler>();
            services.AddTransient<CheckPriceIntegrationEventHandler>();

            services.AddSingleton<IEventBus>(sp =>
            {
                EventBusConfig config = new()
                {
                    ConnectionRetryCount = 5,
                    EventNameSuffix = "IntegrationEvent",
                    SubscriberClientAppName = "ToErp.APP",
                    EventBusType = EventBusType.RabbitMQ,
                    EventBusConnectionString = Configuration.GetSection("RabbitMQConnectionString")?.Value
                };
                return EventBusFactory.Create(config, sp);
            });

            services.AddSingleton<IErpIntegration, OdooErpIntegration>();
            services.AddOptions();
        }
        private static void ConfigurationService(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(sp =>
            {
                return new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json", optional: false) //  appsettings.Development yok ise o zaman appsetting.json oku                                      //.AddEnvironmentVariables()
                 .Build();
            });
            var provider = services.BuildServiceProvider();

            using (var scope = provider.CreateScope())
            {
                Configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            }

        }
        #endregion

        #region events
        private static void OnExit(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("ToErp.APP closed.");
            autoEvent.Set();
            Environment.Exit(0);
        }
        #endregion
    }
}
