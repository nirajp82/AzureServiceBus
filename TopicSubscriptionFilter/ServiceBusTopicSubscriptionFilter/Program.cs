using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace ServiceBusTopicSubscriptionFilter
{
    class Program
    {
        static readonly string _ORDERS_TOPIC_PATH = "Orders";
        static readonly ServiceBusConfig _serviceBusConfig = null;

        static Program()
        {
            _serviceBusConfig = InitServiceBusConfig();
        }

        static async Task Main(string[] args)
        {

            Console.WriteLine("Topics and Subscriptions Console");

            PrompAndWait("Press enter to create topic and subscriptions...");
            await CreateTopicsAndSubscriptions();

            PrompAndWait("Press enter to send order messages...");
            TopicSender topicSender = new TopicSender(_serviceBusConfig.ConnectionString, _ORDERS_TOPIC_PATH);
            await topicSender.SendOrderMessages(Order.CreateOrders());

            PrompAndWait("Press enter to receive order messages...");
            await ReceiveOrdersFromAllSubscriptions();

            PrompAndWait("Topics and Subscriptions Console Complete");
        }

        static async Task CreateTopicsAndSubscriptions()
        {
            var manager = new Manager(_serviceBusConfig.ConnectionString);

            await manager.CreateTopicAsync(_ORDERS_TOPIC_PATH);
            await manager.CreateSubscriptionAsync(_ORDERS_TOPIC_PATH, "AllOrders");

            await manager.CreateSqlFilterSubscription(_ORDERS_TOPIC_PATH, "UsaOrders", "region = 'USA'");
            await manager.CreateSqlFilterSubscription(_ORDERS_TOPIC_PATH, "EuOrders", "region = 'EU'");

            await manager.CreateSqlFilterSubscription(_ORDERS_TOPIC_PATH, "LargeOrders", "items > 30");
            await manager.CreateSqlFilterSubscription(_ORDERS_TOPIC_PATH, "HighValueOrders", "value > 500");

            await manager.CreateSqlFilterSubscription(_ORDERS_TOPIC_PATH, "LoyaltyCardOrders", "loyalty = true AND region = 'USA'");

            await manager.CreateCorrelationFilterSubscription(_ORDERS_TOPIC_PATH, "UkOrders", "UK");
        }

        private static async Task ReceiveOrdersFromAllSubscriptions()
        {
            var manager = new Manager(_serviceBusConfig.ConnectionString);
            foreach (var item in await manager.GetSubscriptionsForTopic(_ORDERS_TOPIC_PATH))
            {
                SubscriptionReceiver subscriptionReceiver = new SubscriptionReceiver(_serviceBusConfig.ConnectionString, item.TopicPath, item.SubscriptionName);
                subscriptionReceiver.RegisterMessageHandler();
                PrompAndWait($"Receiving orders from { item.SubscriptionName }, press enter when complete..");
                await subscriptionReceiver.Close();
            }
        }

        private static ServiceBusConfig InitServiceBusConfig()
        {
            IConfigurationRoot config = BuildConfiguration();
            var serviceBusSection = config.GetSection("ServiceBus");
            var serviceBusConfig = serviceBusSection.Get<ServiceBusConfig>();
            return serviceBusConfig;
        }

        private static IConfigurationRoot BuildConfiguration()
        {
            return new ConfigurationBuilder()
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddUserSecrets<Program>()
                        .Build();
        }

        static void PrompAndWait(string text)
        {
            var temp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(text);
            Console.ForegroundColor = temp;
            Console.ReadLine();
        }
    }
}
