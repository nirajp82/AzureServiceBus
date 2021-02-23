using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceBusConfig serviceBusConfig = InitServiceBusConfig();
            SetupChat(serviceBusConfig);
        }

        private static void SetupChat(ServiceBusConfig serviceBusConfig)
        {
            ManagementClient manager = new ManagementClient(serviceBusConfig.ConnectionString);

            //Create a Topic if it does not exists
            if (!manager.TopicExistsAsync(serviceBusConfig.TopicName).Result)
                manager.CreateTopicAsync(serviceBusConfig.TopicName).Wait();

            //Create new subscription for each user. Message will be delivered to each subscription.
            string userName = GetUserName();
            while (manager.SubscriptionExistsAsync(serviceBusConfig.TopicName, userName).Result)
            {
                Console.WriteLine("Please choose different user name");
                userName = GetUserName();
            }
            SubscriptionDescription subDesc = new SubscriptionDescription(serviceBusConfig.TopicName, userName)
            {
                AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
            };
            manager.CreateSubscriptionAsync(subDesc).Wait();

            //Create clients
            TopicClient senderClient = new TopicClient(serviceBusConfig.ConnectionString, serviceBusConfig.TopicName);
            SubscriptionClient receiverClient = new SubscriptionClient(serviceBusConfig.ConnectionString, serviceBusConfig.TopicName, userName);

            //Create a message pump for receiving & processing messages
            receiverClient.RegisterMessageHandler(ProcessMessagesAsync, ExceptionHandlerAsync);

            //Send a message
            var helloMessage = new Message(Encoding.UTF8.GetBytes("Has entered the room..."))
            {
                Label = userName
            };
            senderClient.SendAsync(helloMessage).Wait();

            while (true)
            {
                string text = Console.ReadLine();
                if (text.Equals("exit"))
                    break;

                var chatMessage = new Message(Encoding.UTF8.GetBytes(text))
                {
                    Label = userName
                };
                senderClient.SendAsync(chatMessage).Wait();
            }

            var byeMessage = new Message(Encoding.UTF8.GetBytes("Has left the building..."))
            {
                Label = userName
            };
            senderClient.SendAsync(byeMessage).Wait();

            senderClient.CloseAsync().Wait();
            receiverClient.CloseAsync().Wait();
        }

        private static async Task ProcessMessagesAsync(Microsoft.Azure.ServiceBus.Message msg, CancellationToken token)
        {
            Console.WriteLine($"{msg.Label} > {Encoding.UTF8.GetString(msg.Body)}");
        }

        private static async Task ExceptionHandlerAsync(ExceptionReceivedEventArgs arg)
        {
            Console.WriteLine(arg.ToString());
        }

        private static string GetUserName()
        {
            Console.WriteLine("Enter name:");
            return Console.ReadLine();
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
