using MessageEntity;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Message.Receiver
{
    class Program
    {
        static readonly QueueClient _queueClient = null;
        static readonly ServiceBusConfig _serviceBusConfig = null;

        static Program()
        {
            ServiceBusConfig serviceBusConfig = InitServiceBusConfig();
            _queueClient = new QueueClient(serviceBusConfig.ConnectionString, serviceBusConfig.QueueName);
        }

        static void Main()
        {
            ProcessMessageWithOptions();
        }

        static void ProcessMessageWithOptions()
        {
            var options = new MessageHandlerOptions(ExceptionHandlerAsync)
            {
                AutoComplete = false,
                MaxConcurrentCalls = 5,
                MaxAutoRenewDuration = TimeSpan.FromMinutes(10)
            };
            _queueClient.RegisterMessageHandler(MessageHandlerWithOptionsAsync, options);
            Console.ReadLine();
            _queueClient.CloseAsync().Wait();
            Console.WriteLine("Messages are Processed");
        }

        static async Task MessageHandlerWithOptionsAsync(Microsoft.Azure.ServiceBus.Message msg, CancellationToken token)
        {
            if (msg.Label == "NewPizzaOrder")
                await ProcessPizzaOrderMessages(msg);
            else if (msg.Label == "ControlMessage_MessageWithoutBody")
                await ProcessControlMessages(msg);
            else
                Console.WriteLine(Encoding.UTF8.GetString(msg.Body));
            await _queueClient.CompleteAsync(msg.SystemProperties.LockToken);
        }

        static void ProcessMessages(ServiceBusConfig serviceBusConfig)
        {
            QueueClient queueClient = new QueueClient(serviceBusConfig.ConnectionString, serviceBusConfig.QueueName);
            queueClient.RegisterMessageHandler(MessageHandlerAsync, ExceptionHandlerAsync);
            Console.ReadLine();
            queueClient.CloseAsync().Wait();
            Console.WriteLine("Messages are Processed");
        }

        static async Task MessageHandlerAsync(Microsoft.Azure.ServiceBus.Message msg, CancellationToken token)
        {
            if (msg.Label == "NewPizzaOrder")
                await ProcessPizzaOrderMessages(msg);
            else if (msg.Label == "ControlMessage_MessageWithoutBody")
                await ProcessControlMessages(msg);
            else
                Console.WriteLine(Encoding.UTF8.GetString(msg.Body));
        }

        static async Task ProcessPizzaOrderMessages(Microsoft.Azure.ServiceBus.Message msg)
        {
            PizzaOrder pizzaOrder = JsonSerializer.Deserialize<PizzaOrder>(Encoding.UTF8.GetString(msg.Body));
            //Cook Pizza
            Console.WriteLine($"Cooking {pizzaOrder.Size} {pizzaOrder.Type} Pizza for {pizzaOrder.CustomerName}");
            Thread.Sleep(2000);
            Console.WriteLine($"   {pizzaOrder.Size} {pizzaOrder.Type} Pizza for {pizzaOrder.CustomerName} is ready!");
        }

        static async Task ProcessControlMessages(Microsoft.Azure.ServiceBus.Message msg)
        {
            foreach (var item in msg.UserProperties)
            {
                Console.WriteLine($"{item.Key}- {item.Value}");
            }
        }

        static async Task ExceptionHandlerAsync(ExceptionReceivedEventArgs arg)
        {
            Console.WriteLine(arg.ToString());
        }

        static ServiceBusConfig InitServiceBusConfig()
        {
            IConfigurationRoot config = BuildConfiguration();
            var serviceBusSection = config.GetSection("ServiceBus");
            var serviceBusConfig = serviceBusSection.Get<ServiceBusConfig>();
            return serviceBusConfig;
        }

        static IConfigurationRoot BuildConfiguration()
        {
            return new ConfigurationBuilder()
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddUserSecrets<Program>()
                        .Build();
        }
    }
}