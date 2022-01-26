using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
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
        readonly IConfiguration _configuration;
        readonly ILogger<CacheSynchronizerService> _loggger;
        readonly string _topicName;
        #endregion


        #region Constructor
        public CacheSynchronizerService(IConfiguration configuration, ILogger<CacheSynchronizerService> loggger, ServiceBusClient sbClient)
        {
            _configuration = configuration;
            _loggger = loggger;
            _topicName = configuration.GetValue<string>("Azure:ServiceBus:SourceSynchronizer:TopicName");
            _sbClient = sbClient;
            _sbSender = _sbClient.CreateSender(_topicName);
        }
        #endregion


        #region Public Methods
        public async Task CreateTopicAsync()
        {
            var connString = _configuration.GetValue<string>("Azure:ServiceBus:SourceSynchronizer:ConnString");
            var sbAdminClient = new ServiceBusAdministrationClient(connString);
            if (!await sbAdminClient.TopicExistsAsync(_topicName))
                await sbAdminClient.CreateTopicAsync(_topicName);
        }

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
                message.ApplicationProperties["Client"] = _configuration.GetValue<string>("Azure:ServiceBus:SourceSynchronizer:RuleOptions:ClientName");
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


        #region Private Methods

        #endregion
    }
}