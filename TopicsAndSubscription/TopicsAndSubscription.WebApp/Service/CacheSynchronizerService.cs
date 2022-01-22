using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TopicsAndSubscription.WebApp
{
    public class CacheSynchronizerService : IDisposable, ICacheSynchronizerService
    {
        #region Private Members
        // the client that owns the connection and can be used to create sender.
        readonly ServiceBusClient _sbClient;
        readonly ServiceBusSender _sbSender;

        readonly ILogger<CacheSynchronizerService> _loggger;
        #endregion


        #region Constructor
        public CacheSynchronizerService(IConfiguration configuration, ILogger<CacheSynchronizerService> loggger, ServiceBusClient sbClient)
        {
            _loggger = loggger;

            var topicName = configuration.GetValue<string>("Azure:ServiceBus:SourceSynchronizer:TopicName");
            //var connectionString = configuration.GetValue<string>("Azure:ServiceBus:SourceSynchronizer:ConnString");
            //_sbClient = new ServiceBusClient(connectionString);
            _sbClient = sbClient;
            _sbSender = _sbClient.CreateSender(topicName);
        }
        #endregion


        #region Public Methods
        public async Task SendSyncRequestAsync(string clientName)
        {
            try
            {
                var data = new
                {
                    Client = clientName,
                    RequestDate = DateTime.UtcNow
                };
                var message = new ServiceBusMessage(JsonSerializer.Serialize(data));
                await _sbSender.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                _loggger.LogError(ex, $"{nameof(SendSyncRequestAsync)} - {clientName}");
            }
        }

        public void Dispose()
        {
            _sbSender.DisposeAsync().GetAwaiter().GetResult();
        }
        #endregion
    }
}