using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServiceBusTopicSubscriptionFilter
{
    public class TopicSender
    {
        readonly TopicClient _topicClient;

        public TopicSender(string connectionString, string topicPath)
        {
            _topicClient = new TopicClient(connectionString, topicPath);
        }

        public async Task SendOrderMessages(IEnumerable<Order> orders)
        {
            foreach (Order order in orders)
            {
                Console.WriteLine($"{ order }");

                var orderJSON = JsonSerializer.Serialize(order);
                Message message = new Message(Encoding.UTF8.GetBytes(orderJSON));

                //Promote properties to header level that willl be used to apply filter
                message.UserProperties.Add("region", order.Region);
                message.UserProperties.Add("items", order.Items);
                message.UserProperties.Add("value", order.Value);            
                message.UserProperties.Add("loyalty", order.HasLoyltyCard);

                // Set the correlation Id
                message.CorrelationId = order.Region;

                // Send the message
                await _topicClient.SendAsync(message);
            }

            await _topicClient.CloseAsync();
        }
    }
}
