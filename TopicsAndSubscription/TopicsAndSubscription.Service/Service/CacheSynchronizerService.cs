using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TopicsAndSubscription.Service
{
    public class CacheSynchronizerService : IDisposable, ICacheSynchronizerService
    {
        #region Private Members
        // the sender used to publish messages to the topic
        readonly string _topicName;
        readonly string _subscriptionName;

        // the client that owns the connection and can be used to create receivers
        readonly ServiceBusClient _sbClient;
        //readonly ServiceBusManagementClient _sbManager;
        // the processor that reads and processes messages from the subscription
        readonly ServiceBusProcessor _sbProcessor;
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

            _subscriptionName = CreateSubscriptionAsync().GetAwaiter().GetResult();
            //create a processor that will be used to process the messages
            _sbProcessor = _sbClient.CreateProcessor(_topicName, _subscriptionName, new ServiceBusProcessorOptions());

            // add handler to process messages
            _sbProcessor.ProcessMessageAsync += MessageHandler;

            // add handler to process any errors
            _sbProcessor.ProcessErrorAsync += ErrorHandler;

            //start processing 
            _sbProcessor.StartProcessingAsync().GetAwaiter().GetResult();
        }

        #endregion


        #region Public Methods        
        public void Dispose()
        {
            _sbProcessor.StopProcessingAsync().GetAwaiter().GetResult();
            _sbClient.DisposeAsync().GetAwaiter().GetResult();
            DeleteSubscriptionAsync().GetAwaiter().GetResult();
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
            var subscriptionName = $"{DateTime.UtcNow.ToString("MM-dd-yyyy")}-{Guid.NewGuid()}";
            var subscriptionOptions = new CreateSubscriptionOptions(_topicName, subscriptionName)
            {
                AutoDeleteOnIdle = TimeSpan.FromDays(7),
                DefaultMessageTimeToLive = TimeSpan.FromMinutes(60),
                EnableBatchedOperations = true,
                //UserMetadata = "TODO:"
            };
            var createdSubscription = await _sbAdminClient.CreateSubscriptionAsync(subscriptionOptions);
            return subscriptionName;
        }

        async Task DeleteSubscriptionAsync()
        {
            await _sbAdminClient.CreateSubscriptionAsync(_topicName, _subscriptionName);
        }
        #endregion
    }
}