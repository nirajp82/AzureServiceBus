using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


using System;
using System.Text;

namespace Message.Sender
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceBusConfig serviceBusConfig = InitServiceBusConfig();
            SendMessages(serviceBusConfig);
            Console.WriteLine("Messages are sent");
        }

        private static void SendMessages(ServiceBusConfig serviceBusConfig)
        {
            QueueClient queueClient = new QueueClient(serviceBusConfig.ConnectionString, serviceBusConfig.QueueName);
            for (int idx = 1; idx <= 10; idx++)
            {
                string msgBody = $"{idx}: Current DateTime: {DateTime.Now.ToString()}";
                var message = new Microsoft.Azure.ServiceBus.Message
                {
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

    class ServiceBusConfig
    {
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
    }
}
