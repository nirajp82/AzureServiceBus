using System;
using System.Configuration;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace TopicsAndSubscription.Service.Framework.Service
{
    public class CacheSynchronizerService
    {
        #region Private Members
        // the sender used to publish messages to the topic
        static readonly string _connString;
        static readonly string _topicName;
        static string _subscriptionName;
        // the client that owns the connection and can be used to create receivers
        static readonly ServiceBusClient _sbClient;
        //readonly ServiceBusManagementClient _sbManager;
        // the processor that reads and processes messages from the subscription
        static ServiceBusProcessor _sbProcessor;
        static readonly ServiceBusAdministrationClient _sbAdminClient;
        #endregion


        #region Constructor
        static CacheSynchronizerService()
        {
            _topicName = ConfigurationManager.AppSettings["Azure:ServiceBus:SourceSynchronizer:TopicName"];
            _connString = ConfigurationManager.AppSettings["Azure:ServiceBus:SourceSynchronizer:ConnString"];
            _sbClient = new ServiceBusClient(_connString);
            _sbAdminClient = new ServiceBusAdministrationClient(_connString);
        }

        #endregion


        #region Public Methods
        public static async Task StartAsync()
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

        public static async Task StopAsync()
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
        static async Task MessageHandler(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            CacheContainerService.RefreshCache(body);
            // complete the message. messages is deleted from the subscription. 
            await args.CompleteMessageAsync(args.Message);
        }

        // handle any errors when receiving messages
        static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            //_loggger.LogError(args.Exception, nameof(ErrorHandler));
            return Task.CompletedTask;
        }


        static async Task<string> CreateSubscriptionAsync()
        {
            var subscriptionName = $"{DateTime.UtcNow.ToString("MM-dd-yyyy-HH")}-{Guid.NewGuid()}";
            var subscriptionOptions = new CreateSubscriptionOptions(_topicName, subscriptionName)
            {
                AutoDeleteOnIdle = TimeSpan.FromMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["Azure:ServiceBus:SourceSynchronizer:SubscriptionOptions:AutoDeleteOnIdle"])),
                DefaultMessageTimeToLive = TimeSpan.FromMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["Azure:ServiceBus:SourceSynchronizer:SubscriptionOptions:DefaultMessageTimeToLive"])),
                EnableBatchedOperations = true,
            };
            var ruleOptions = new CreateRuleOptions { Name = "TargetClient", Filter = new SqlRuleFilter($"Client = '{ConfigurationManager.AppSettings["Azure:ServiceBus:SourceSynchronizer:RuleOptions:ClientName"]}'") };
            var createdSubscription = await _sbAdminClient.CreateSubscriptionAsync(subscriptionOptions, ruleOptions);
            return createdSubscription.Value.SubscriptionName;
        }

        static async Task DeleteSubscriptionAsync()
        {
            if (await _sbAdminClient.SubscriptionExistsAsync(_topicName, _subscriptionName))
                await _sbAdminClient.DeleteSubscriptionAsync(_topicName, _subscriptionName);
        }
        #endregion
    }
}