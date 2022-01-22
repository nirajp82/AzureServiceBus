using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TopicsAndSubscription.Service
{
    public class CacheSynchronizerService : ICacheSynchronizerService
    {
        #region Private Members
        // the sender used to publish messages to the topic
        readonly string _topicName;
        string _subscriptionName;

        // the client that owns the connection and can be used to create receivers
        readonly ServiceBusClient _sbClient;
        //readonly ServiceBusManagementClient _sbManager;
        // the processor that reads and processes messages from the subscription
        ServiceBusProcessor _sbProcessor;
        readonly ServiceBusAdministrationClient _sbAdminClient;

        readonly IConfiguration _configuration;
        readonly ILogger<CacheSynchronizerService> _loggger;
        readonly ICacheContainerService _cacheContainerService;
        #endregion


        #region Constructor
        public CacheSynchronizerService(IConfiguration configuration, ILogger<CacheSynchronizerService> loggger,
            ICacheContainerService cacheContainerService, ServiceBusClient sbClient)
        {
            _configuration = configuration;
            _loggger = loggger;
            _cacheContainerService = cacheContainerService;

            _topicName = configuration.GetValue<string>("Azure:ServiceBus:SourceSynchronizer:TopicName");
            _sbClient = sbClient;
            _sbAdminClient = new ServiceBusAdministrationClient(configuration.GetValue<string>("Azure:ServiceBus:SourceSynchronizer:ConnString"));
        }

        #endregion


        #region Public Methods
        public async Task StartAsync()
        {
            _subscriptionName = await CreateSubscriptionAsync();
            //create a processor that will be used to process the messages
            _sbProcessor = _sbClient.CreateProcessor(_topicName, _subscriptionName, new ServiceBusProcessorOptions());

            // add handler to process messages
            _sbProcessor.ProcessMessageAsync += MessageHandler;

            // add handler to process any errors
            _sbProcessor.ProcessErrorAsync += ErrorHandler;

            //start processing 
            await _sbProcessor.StartProcessingAsync();
        }

        public async Task StopAsync()
        {
            if (string.IsNullOrWhiteSpace(_subscriptionName))
                return;

            var tskStopProcessing = _sbProcessor.StopProcessingAsync();
            var tskDeleteSub = DeleteSubscriptionAsync();
            await _sbClient.DisposeAsync();
            await Task.WhenAll(tskStopProcessing, tskDeleteSub);
            _subscriptionName = null;
        }

        public void Dispose()
        {
            StopAsync().GetAwaiter().GetResult();
        }
        #endregion


        #region Private Methods
        // handle received messages
        async Task MessageHandler(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            _cacheContainerService.RefreshCache(body);
            // complete the message. messages is deleted from the subscription. 
            await args.CompleteMessageAsync(args.Message);
        }

        // handle any errors when receiving messages
        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _loggger.LogError(args.Exception, nameof(ErrorHandler));
            return Task.CompletedTask;
        }

        async Task<string> CreateSubscriptionAsync()
        {
            var subscriptionName = $"{DateTime.UtcNow.ToString("MM-dd-yyyy-HH")}-{Guid.NewGuid()}";
            var subscriptionOptions = new CreateSubscriptionOptions(_topicName, subscriptionName)
            {
                AutoDeleteOnIdle = TimeSpan.FromDays(7),
                DefaultMessageTimeToLive = TimeSpan.FromMinutes(60),
                EnableBatchedOperations = true,
                //UserMetadata = "TODO:"
            };
            var createdSubscription = await _sbAdminClient.CreateSubscriptionAsync(subscriptionOptions);
            return createdSubscription.Value.SubscriptionName;
        }

        async Task DeleteSubscriptionAsync()
        {
            if (await _sbAdminClient.SubscriptionExistsAsync(_topicName, _subscriptionName))
                await _sbAdminClient.DeleteSubscriptionAsync(_topicName, _subscriptionName);
        }
        #endregion
    }
}