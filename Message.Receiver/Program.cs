using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Message.Receiver
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceBusConfig serviceBusConfig = InitServiceBusConfig();
            ProcessMessages(serviceBusConfig);
        }

        private static void ProcessMessages(ServiceBusConfig serviceBusConfig)
        {
            QueueClient queueClient = new QueueClient(serviceBusConfig.ConnectionString, serviceBusConfig.QueueName);
            queueClient.RegisterMessageHandler(MessageHandlerAsync, ExceptionHandlerAsync);
            Console.ReadLine();
            queueClient.CloseAsync().Wait();
            Console.WriteLine("Messages are Processed");
        }

        private static async Task MessageHandlerAsync(Microsoft.Azure.ServiceBus.Message msg, CancellationToken token)
        {
            Console.WriteLine(Encoding.UTF8.GetString(msg.Body));
        }

        private static async Task ExceptionHandlerAsync(ExceptionReceivedEventArgs arg)
        {
            Console.WriteLine(arg.ToString());
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