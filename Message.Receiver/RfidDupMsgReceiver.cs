using MessageEntity;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Message.Receiver
{

    class RfidDupMsgReceiver
    {
        const string _QueueName = "RfidQueue";
        static readonly QueueClient _queueClient = null;
        static readonly ICollection<RfidTag> list = new List<RfidTag>();

        static RfidDupMsgReceiver()
        {
            ServiceBusConfig serviceBusConfig = Program.InitServiceBusConfig();
            _queueClient = new QueueClient(serviceBusConfig.ConnectionString, _QueueName);
        }

        internal static void Start()
        {
            Console.WriteLine($"Waiting for Items (messages) .");
            _queueClient.RegisterMessageHandler(MessageHandler, ExceptionHandler);
            Console.WriteLine($"Press Enter when ready");
            Console.ReadLine();
            _queueClient.CloseAsync().Wait();
            Console.WriteLine($"Total {list.Count()} Items received, Cost: {list.Sum(p => p.Price)}");
        }

        static Task MessageHandler(Microsoft.Azure.ServiceBus.Message msg, CancellationToken token)
        {
            string itemJson = Encoding.UTF8.GetString(msg.Body);
            RfidTag tag = JsonSerializer.Deserialize<RfidTag>(itemJson);
            Console.WriteLine($"Item Received: {tag.Product} Price: {tag.Price}");
            list.Add(tag);

            return Task.CompletedTask;
        }

        static Task ExceptionHandler(ExceptionReceivedEventArgs arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }
    }
}
