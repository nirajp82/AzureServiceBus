using MessageEntity;
//using ASB = Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Message.Sender
{
    class Program
    {
        static void Main()
        {
            ServiceBusConfig serviceBusConfig = InitServiceBusConfig();
            //SendTextMessages(serviceBusConfig);
            SendPizzaOrderAsync(serviceBusConfig).Wait();
            SendControlMessageAsync(serviceBusConfig).Wait();
            Console.WriteLine("Messages are sent");
        }

        static async Task SendControlMessageAsync(ServiceBusConfig serviceBusConfig)
        {
            Console.WriteLine("Sending Control Message (Message without body)", ConsoleColor.DarkCyan);
            var message = new Microsoft.Azure.ServiceBus.Message()
            {
                Label = "ControlMessage_MessageWithoutBody"
            };
            message.UserProperties.Add("Id", 1324);
            message.UserProperties.Add("Action", "Create");
            message.UserProperties.Add("ActionTime", DateTime.Now);

            //Send Order
            QueueClient queueClient = new QueueClient(serviceBusConfig.ConnectionString, serviceBusConfig.QueueName);
            Console.WriteLine("Sending Control Message (Message without body)", ConsoleColor.Green);
            await queueClient.SendAsync(message);
            Console.WriteLine("Control Message (Message without body) Sent", ConsoleColor.Green);
            await queueClient.CloseAsync();
        }

        static async Task SendPizzaOrderAsync(ServiceBusConfig serviceBusConfig)
        {
            Console.WriteLine("Sending Pizza Order", ConsoleColor.DarkCyan);
            IList<Microsoft.Azure.ServiceBus.Message> msgList = new List<Microsoft.Azure.ServiceBus.Message>();
            foreach (var name in new List<string> { "John Doe", "Jane Doe" })
            {
                foreach (var size in new List<string> { "Large", "Medium", "Size" })
                {
                    PizzaOrder pizzaOrder = new PizzaOrder()
                    {
                        CustomerName = name,
                        Size = size,
                        Type = "Veggi"
                    };
                    string pizzaOrderJSON = JsonSerializer.Serialize(pizzaOrder);
                    var message = new Microsoft.Azure.ServiceBus.Message(Encoding.UTF8.GetBytes(pizzaOrderJSON))
                    {
                        Label = "NewPizzaOrder",
                        ContentType = "application/json"
                    };
                    msgList.Add(message);
                }
            }
            //Send Order
            QueueClient queueClient = new QueueClient(serviceBusConfig.ConnectionString, serviceBusConfig.QueueName);
            Console.WriteLine("Sending Pizza Order", ConsoleColor.Green);
            await queueClient.SendAsync(msgList);
            Console.WriteLine("Pizza Order Sent", ConsoleColor.Green);
            await queueClient.CloseAsync();
        }

        private static void SendTextMessages(ServiceBusConfig serviceBusConfig)
        {
            QueueClient queueClient = new QueueClient(serviceBusConfig.ConnectionString, serviceBusConfig.QueueName);
            for (int idx = 1; idx <= 10; idx++)
            {
                string msgBody = $"{idx}: Current DateTime: {DateTime.Now}";
                var message = new Microsoft.Azure.ServiceBus.Message
                {
                    Label = "SampleMesage",
                    Body = Encoding.UTF8.GetBytes(msgBody)
                };
                Console.WriteLine($"Sending Message: {msgBody}");
                queueClient.SendAsync(message).Wait();
            }
            queueClient.CloseAsync().Wait();
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
    }
}
