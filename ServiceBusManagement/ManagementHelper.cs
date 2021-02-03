using Microsoft.Azure.ServiceBus.Management;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBus.Management
{
    public class ManagementHelper
    {
        private ManagementClient _managementClient { get; }

        public ManagementHelper(string connString)
        {
            _managementClient = new ManagementClient(connString);
        }

        public async Task CreateQueueAsync(string queuePath)
        {
            QueueDescription queueDesc = new QueueDescription(queuePath)
            {
                RequiresDuplicateDetection = true,
                DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(5),
                RequiresSession = true,
                MaxDeliveryCount = 10,
                DefaultMessageTimeToLive = TimeSpan.FromHours(1),
                EnableDeadLetteringOnMessageExpiration = true
            };
            QueueDescription response = await _managementClient.CreateQueueAsync(queueDesc);
            Console.WriteLine($"\t Create queue status: \"{response.Status}\"");
        }

        public async Task DeleteQueueAsync(string queuePath)
        {
            await _managementClient.DeleteQueueAsync(queuePath);
            Console.WriteLine($"\t Queue \"{queuePath}\" has been Deleted!");
        }

        public async Task ListQueuesAsync()
        {
            IEnumerable<QueueDescription> queueDescriptions = await _managementClient.GetQueuesAsync();
            foreach (var item in queueDescriptions)
                Console.WriteLine($"\t {item.Path}");
        }

        public async Task GetQueueAsync(string queuePath)
        {
            QueueDescription queueDescription = await _managementClient.GetQueueAsync(queuePath);
            Console.WriteLine($"\tQueue description for {queueDescription.Path}");
            Console.WriteLine($"\t\t Path: {queueDescription.Path}");
            Console.WriteLine($"\t\t DefaultMessageTimeToLive: {queueDescription.DefaultMessageTimeToLive}");
            Console.WriteLine($"\t\t DuplicateDetectionHistoryTimeWindow: {queueDescription.DuplicateDetectionHistoryTimeWindow}");
            Console.WriteLine($"\t\t EnableBatchedOperations: {queueDescription.EnableBatchedOperations}");
            Console.WriteLine($"\t\t EnableDeadLetteringOnMessageExpiration: {queueDescription.EnableDeadLetteringOnMessageExpiration}");
            Console.WriteLine($"\t\t EnablePartitioning: {queueDescription.EnablePartitioning}");
            Console.WriteLine($"\t\t ForwardDeadLetteredMessagesTo: {queueDescription.ForwardDeadLetteredMessagesTo}");
            Console.WriteLine($"\t\t ForwardTo: {queueDescription.ForwardTo}");
            Console.WriteLine($"\t\t LockDuration: {queueDescription.LockDuration}");
            Console.WriteLine($"\t\t MaxDeliveryCount: {queueDescription.MaxDeliveryCount}");
            Console.WriteLine($"\t\t MaxSizeInMB: {queueDescription.MaxSizeInMB}");
            Console.WriteLine($"\t\t RequiresDuplicateDetection: {queueDescription.RequiresDuplicateDetection}");
            Console.WriteLine($"\t\t RequiresSession: {queueDescription.RequiresSession}");
            Console.WriteLine($"\t\t Status: {queueDescription.Status}");
            Console.WriteLine($"\t\t UserMetadata: {queueDescription.UserMetadata}");
        }

        public async Task ListTopicsAndSubscriptionsAsync()
        {
            IEnumerable<TopicDescription> topicDescriptions = await _managementClient.GetTopicsAsync();
            Console.WriteLine($"\t Listing topics and subscriptions...");
            foreach (var topicDescription in topicDescriptions)
            {
                Console.WriteLine($"\t Topic:{topicDescription.Path}");
                IEnumerable<SubscriptionDescription> subscriptionDescriptions = await _managementClient.GetSubscriptionsAsync(topicDescription.Path);
                foreach (SubscriptionDescription subscriptionDescription in subscriptionDescriptions)
                    Console.WriteLine("\t\t{0}", subscriptionDescription.SubscriptionName);
            }
        }

        public async Task CreateTopicAsync(string topicPath)
        {
            TopicDescription response = await _managementClient.CreateTopicAsync(topicPath);
            Console.WriteLine($"\t Create topic status: \"{response.Status}\"");
        }

        public async Task CreateSubscriptionAsync(string topicPath, string subscriptionName)
        {
            SubscriptionDescription response = await _managementClient.CreateSubscriptionAsync(topicPath, subscriptionName);
            Console.WriteLine($"\t Create SubscriptionDescription status: \"{response.Status}\"");
        }
    }
}
