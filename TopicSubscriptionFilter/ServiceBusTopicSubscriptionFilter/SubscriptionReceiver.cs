using Microsoft.Azure.ServiceBus;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceBusTopicSubscriptionFilter
{
    public class SubscriptionReceiver
    {
        private SubscriptionClient _subscriptionClient;

        public SubscriptionReceiver(string connectionString, string topicPath, string subscriptionName)
        {
            _subscriptionClient = new SubscriptionClient(connectionString, topicPath, subscriptionName);
        }

        public void RegisterMessageHandler()
        {
            MessageHandlerOptions options = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            _subscriptionClient.RegisterMessageHandler(ProcessOrderMessageMessageAsync, options);
        }

        public async Task Close()
        {
            await _subscriptionClient.CloseAsync();
        }

        private async Task ProcessOrderMessageMessageAsync(Message msg, CancellationToken arg2)
        {
            Order order = JsonSerializer.Deserialize<Order>(Encoding.UTF8.GetString(msg.Body));
            Console.WriteLine($"{order}");

            // Complete the message
            await _subscriptionClient.CompleteAsync(msg.SystemProperties.LockToken);
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            return Task.CompletedTask;
        }
    }
}
