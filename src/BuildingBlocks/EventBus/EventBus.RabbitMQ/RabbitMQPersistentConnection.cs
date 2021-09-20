using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.RabbitMQ
{
    public class RabbitMQPersistentConnection : IDisposable
    {

        #region fields 
        private IConnection _connection;
        private readonly IConnectionFactory _connectionFactory;
        private readonly int _retryCount;
        private object _lock = new();
        private bool _disposed;
        private ILogger _logger;
        private string _connectionString;

        #endregion

        #region ctor
        public RabbitMQPersistentConnection(IConnectionFactory connectionFactory, ILogger logger, string connectionString, int retryCount = 5)
        {
            _connectionString = connectionString;
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _retryCount = retryCount;
            _logger = logger;
        }
        #endregion

        #region props 
        public bool IsConnected => _connection != null && _connection.IsOpen;

        #endregion

        #region  Public methods

        public IModel CreateModel()
        {
            return _connection.CreateModel();
        }

        public bool TryConnect()
        {
            lock (_lock)
            {
                var policy = Policy.Handle<SocketException>().Or<BrokerUnreachableException>()
                    .WaitAndRetry(_retryCount, retryAttemp => TimeSpan.FromSeconds(Math.Pow(2, retryAttemp)), (ex, time) =>
                       {
                           //Logging mechasim
                       });
                policy.Execute(() =>
                {
                    _connection = _connectionFactory.CreateConnection(_connectionString);
                });

                if (IsConnected)
                {
                    // log 
                    _connection.ConnectionShutdown += Connection_ConnectionShutdown;
                    _connection.CallbackException += Connection_CallbackException;
                    _connection.ConnectionBlocked += Connection_ConnectionBlocked;
                    return true;
                }
                return false;
            }
        }


        #endregion

        #region events methods 
        private void Connection_ConnectionBlocked(object sender, global::RabbitMQ.Client.Events.ConnectionBlockedEventArgs e)
        {
            if (_disposed)
                return;
            _logger.LogWarning($"Connection Blocked Reason : {e.Reason}");
            TryConnect();
        }

        private void Connection_CallbackException(object sender, global::RabbitMQ.Client.Events.CallbackExceptionEventArgs e)
        {
            if (_disposed)
                return;
            _logger.LogError($"Connection CallBack Exception Detail : {e.Detail} Exception Message : {e.Exception.Message}");
            TryConnect();
        }

        private void Connection_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            if (_disposed) return;

            _logger.LogError($"Connection ShutDown : {e.ClassId}.{e.MethodId} Reply Text : {e.ReplyText}");
            TryConnect();
        }

        #endregion

        #region dispose
        public void Dispose()
        {
            _disposed = true;
            _connection.Dispose();
            GC.SuppressFinalize(_connectionFactory);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
