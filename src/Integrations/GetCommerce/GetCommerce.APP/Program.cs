using EventBus.Base;
using EventBus.Base.Abstractions;
using EventBus.Factory;
using GetCommerce.APP.Extensions;
using GetCommerce.APP.IntegrationEvents.Events;
using GetCommerce.APP.IntegrationEvents.Handlers;
using GetCommerce.APP.Integrations.Abstractions;
using GetCommerce.APP.Integrations.Commerce;
using GetCommerce.APP.Models;
using GetCommerce.APP.Models.CommerceModels;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace GetCommerce.APP
{
    class Program
    {
        #region fields and prop
        public static IConfiguration Configuration { get; set; }
        static readonly AutoResetEvent autoEvent = new(false);
        static IEventBus _eventBus;
        static ICommerceIntegration _commerceIntegration;
        #endregion

        #region main methods
        static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();

            ConfigurationService(services);

            ConfigureServices(services);

            var sp = services.BuildServiceProvider();

            //Initialize();

            _eventBus = sp.GetRequiredService<IEventBus>();
            _commerceIntegration = sp.GetRequiredService<ICommerceIntegration>();

            _eventBus.Subcribe<ChangeOrderStatusIntegrationEvent, ChangeOrderStatusIntegrationEventHandler>();

            //StartJobs();
             ProcessNewOrder();

            //ProcessSentOrder();
        //    ProcessCancelledOrders();
          //  using var server = new BackgroundJobServer();

            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);

            autoEvent.WaitOne();

        }
        #endregion
        #region methods

        public static void ProcessNewOrder()
        {
            var orders = _commerceIntegration.GetOrders(OrderStatus.YeniSiparis);

            if (orders != null)
            {
                if (orders.Count <= 0)
                {
                    return;
                }
                _eventBus.Publish(new GetOrdersIntegrationEvent
                {
                    Orders = orders
                });
            }
        }
        public static void ProcessSentOrders()
        {
            var sentOrders = _commerceIntegration.GetOrders(OrderStatus.Gönderildi);

            if (sentOrders != null)
            {
                if (sentOrders.Count <= 0)
                {
                    return;
                }
                //Publish Event
                _eventBus.Publish(new GetSentOrdersIntegrationEvent(sentOrders));
            }
        }

        public static void ProcessCompletedOrders()
        {
            var completedOrders = _commerceIntegration.GetOrders(OrderStatus.Tamamlandi);

            if (completedOrders != null)
            {
                if (completedOrders.Count <= 0)
                {
                    return;
                }
                //Publish Event
                _eventBus.Publish(new GetCompletedOrdersIntegrationEvent(completedOrders));
            }
        }
        public static void ProcessCancelledOrders()
        {
            var cancelledOrder = _commerceIntegration.GetOrders(OrderStatus.İptalEdildi);

            if (cancelledOrder != null)
            {
                if (cancelledOrder.Count <= 0)
                {
                    return;
                }

                _eventBus.Publish(new GetCancelledOrdersIntegrationEvent
                {

                    CancelledOrders = cancelledOrder
                });
            }
        }

        
        public static void StartJobs()
        {
            Console.WriteLine("GetNewOrders Job Triggered. Triggered Time : " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));

            RecurringJob.AddOrUpdate(
               "FetchOrders",
               () => ProcessNewOrder(),
               Configuration.GetSection("HangfireCronExpression").Value);
        }
        private static void Initialize()
        {

            var mongoUrlBuilder = new MongoUrlBuilder(Configuration.GetSection("MongoUrl").Value);

            var mongoClient = new MongoClient(mongoUrlBuilder.ToMongoUrl());

            GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMongoStorage(mongoClient, mongoUrlBuilder.DatabaseName, new MongoStorageOptions
                {
                    MigrationOptions = new MongoMigrationOptions
                    {
                        MigrationStrategy = new MigrateMongoMigrationStrategy(),
                        BackupStrategy = new CollectionMongoBackupStrategy()
                    },
                    Prefix = "hangfire.mongo",
                    CheckConnection = true
                }
            );
        }
        #endregion

        #region events
        private static void OnExit(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("GetCommerce.APP closed.");
            autoEvent.Set();
            Environment.Exit(0);
        }
        #endregion

        #region configurations

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
        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure =>
            {
                configure.AddConsole();
            });

            services.ConfigureMapping();

            services.Configure<CommerceConfig>(settings => Configuration.GetSection(nameof(CommerceConfig)).Bind(settings));

            services.AddTransient<ChangeOrderStatusIntegrationEventHandler>();
            services.AddSingleton<IEventBus>(sp =>
            {
                EventBusConfig config = new()
                {
                    ConnectionRetryCount = 5,
                    EventNameSuffix = "IntegrationEvent",
                    SubscriberClientAppName = "GetCommerce.APP",
                    EventBusType = EventBusType.RabbitMQ
                };
                return EventBusFactory.Create(config, sp);
            });

            services.AddSingleton<ICommerceIntegration, CommerceIntegration>();
            //ChangeOrderStatusIntegrationEvent
            services.AddOptions();
        }

        #endregion

    }
}
