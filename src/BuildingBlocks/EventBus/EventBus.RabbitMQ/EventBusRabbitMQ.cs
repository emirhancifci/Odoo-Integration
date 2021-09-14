using EventBus.Base;
using EventBus.Base.Events;
using EventBus.Service.IntegrationEventService.Events;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
namespace EventBus.RabbitMQ
{
    public class EventBusRabbitMQ : BaseEventBus<EventBusRabbitMQ>
    {
        private readonly RabbitMQPersistentConnection _persistentConnection;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IModel _consumerChannel;
        public EventBusRabbitMQ(EventBusConfig eventBusConfig, IServiceProvider serviceProvider) : base(eventBusConfig, serviceProvider)
        {
            if (EventBusConfig.Connection != null)
            {
                var connJson = JsonConvert.SerializeObject(EventBusConfig.Connection, new JsonSerializerSettings()
                {
                    // Self referencing loop detected for property
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                _connectionFactory = JsonConvert.DeserializeObject<ConnectionFactory>(connJson);
            }
            else
                _connectionFactory = new ConnectionFactory();

            _persistentConnection = new RabbitMQPersistentConnection(_connectionFactory, _logger, EventBusConfig.ConnectionRetryCount);

            _consumerChannel = CreateConsumerChannel();

            _eventBusSubscriptionManager.OnEventRemoved += EventBusSubscriptionManager_OnEventRemoved;
        }

        #region events
        private void EventBusSubscriptionManager_OnEventRemoved(object sender, string eventName)
        {
            eventName = ProcessEventName(eventName);

            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            _consumerChannel.QueueUnbind(queue: eventName, exchange: EventBusConfig.DefaultTopicName, routingKey: eventName);

            if (_eventBusSubscriptionManager.IsEmpty)
            {
                _consumerChannel.Close();
            }
        }
        #endregion


        #region implemantations
        public override void Publish(IntegrationEvent @event)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(EventBusConfig.ConnectionRetryCount, retryAttemp => TimeSpan.FromSeconds(Math.Pow(2, retryAttemp)), (ex, time) =>
                {
                    _logger.LogError($"An error occurred while connection : {ex.Message}");

                });

            var eventName = @event.GetType().Name;
            eventName = ProcessEventName(eventName);

            _consumerChannel.ExchangeDeclare(exchange: EventBusConfig.DefaultTopicName, type: "direct");

            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);

            policy.Execute(() =>
            {
                var properties = _consumerChannel.CreateBasicProperties();
                properties.DeliveryMode = 2; // persistent

                _consumerChannel.BasicPublish(
                    exchange: EventBusConfig.DefaultTopicName,
                    routingKey: eventName,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);
            });
        }

        public override void Subcribe<TEvent, THandler>()
        {
            var eventName = typeof(TEvent).Name;
            eventName = ProcessEventName(eventName);

            if (!_eventBusSubscriptionManager.HasSubscriptionsForEvent(eventName))
            {
                if (!_persistentConnection.IsConnected)
                {
                    _persistentConnection.TryConnect();
                }

                _consumerChannel.QueueDeclare(queue: GetSubscriptionName(eventName),
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _consumerChannel.QueueBind(queue: GetSubscriptionName(eventName),
                    exchange: EventBusConfig.DefaultTopicName,
                    routingKey: eventName);

            }
            _eventBusSubscriptionManager.AddSubscription<TEvent, THandler>();
            StartBasicConsume(eventName);
        }

        public override void UnSubcribe<TEvent, THandler>()
        {
            _eventBusSubscriptionManager.RemoveSubscription<TEvent, THandler>();
        }
        #endregion

        #region private

        private IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var channel = _persistentConnection.CreateModel();
            channel.ExchangeDeclare(exchange: EventBusConfig.DefaultTopicName, type: "direct");

            return channel;
        }

        private void StartBasicConsume(string eventName)
        {
            if (_consumerChannel != null)
            {
                var consumer = new EventingBasicConsumer(_consumerChannel);

                consumer.Received += Consumer_Received;

                _consumerChannel.BasicConsume(
                    queue: GetSubscriptionName(eventName),
                    autoAck: false,
                    consumer: consumer
                    );
            }
        }

        private async void Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;

            eventName = ProcessEventName(eventName);

            var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

            try
            {
                await ProcessEventAsync(eventName, message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Consumer_Recived Error : {ex.Message}");
            }

            _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
        }
        #endregion

    }
}
