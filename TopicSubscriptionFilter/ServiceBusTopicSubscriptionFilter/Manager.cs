using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceBusTopicSubscriptionFilter
{
    public class Manager
    {
        private readonly ManagementClient _managementClient;

        public Manager(string connString)
        {
            _managementClient = new ManagementClient(connString);
        }

        public async Task<TopicDescription> CreateTopicAsync(string topicPath)
        {
            Console.WriteLine($"Creating Topic { topicPath }");

            if (!await _managementClient.TopicExistsAsync(topicPath))
                await _managementClient.CreateTopicAsync(topicPath);

            return await _managementClient.GetTopicAsync(topicPath);
        }

        public async Task<SubscriptionDescription> CreateSubscriptionAsync(string topicPath, string subscriptionName)
        {
            Console.WriteLine($"Creating Subscription { topicPath }/{ subscriptionName }");

            await CreateTopicAsync(topicPath);

            if (!await _managementClient.SubscriptionExistsAsync(topicPath, subscriptionName))
                return await _managementClient.CreateSubscriptionAsync(topicPath, subscriptionName);

            return await _managementClient.GetSubscriptionAsync(topicPath, subscriptionName);
        }

        public async Task<SubscriptionDescription> CreateSqlFilterSubscription(string topicPath, string subscriptionName, string sqlExpression)
        {
            Console.WriteLine($"Creating Subscription with SQL Filter{ topicPath }/{ subscriptionName } ({ sqlExpression })");
            if (!await _managementClient.SubscriptionExistsAsync(topicPath, subscriptionName))
            {
                SubscriptionDescription subDesc = new SubscriptionDescription(topicPath, subscriptionName);
                RuleDescription ruleDesc = new RuleDescription("DemoSQLFilter", new SqlFilter(sqlExpression));
                return await _managementClient.CreateSubscriptionAsync(subDesc, ruleDesc);
            }
            return await _managementClient.GetSubscriptionAsync(topicPath, subscriptionName);
        }

        public async Task<SubscriptionDescription> CreateCorrelationFilterSubscription(string topicPath, string subscriptionName, string correlationId)
        {
            Console.WriteLine($"Creating Subscription with Correlation Filter{ topicPath }/{ subscriptionName } ({ correlationId })");
            if (!await _managementClient.SubscriptionExistsAsync(topicPath, subscriptionName))
            {
                SubscriptionDescription subDesc = new SubscriptionDescription(topicPath, subscriptionName);
                RuleDescription ruleDesc = new RuleDescription("DemoCorrelationFilter", new CorrelationFilter(correlationId));
                return await _managementClient.CreateSubscriptionAsync(subDesc, ruleDesc);
            }
            return await _managementClient.GetSubscriptionAsync(topicPath, subscriptionName);
        }

        public async Task<IList<SubscriptionDescription>> GetSubscriptionsForTopic(string topicPath)
        {
            return await _managementClient.GetSubscriptionsAsync(topicPath);
        }
    }
}
