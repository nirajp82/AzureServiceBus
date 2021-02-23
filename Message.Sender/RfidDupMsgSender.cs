using MessageEntity;
using System;
using System.Collections.Generic;
using System.Text;
//using ASB = Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Microsoft.Azure.ServiceBus.Management;

namespace Message.Sender
{
    public class RfidDupMsgSender
    {
        private const string _queueName = "RfidQueue";

        public static async Task Start()
        {
            Console.WriteLine("Starting Checkout");
            ServiceBusConfig serviceBusConfig = Program.InitServiceBusConfig();

            await ManageQueue(serviceBusConfig);

            QueueClient queueClient = new QueueClient(serviceBusConfig.ConnectionString, _queueName);

            //Items in Basket
            var basket = RfidTag.CreateBasket();
            Console.WriteLine($"Basket contains {basket.Length} items, Total cost: {basket.Sum(p => p.Price)}");

            int itemCnt = 0;
            int sentCnt = 0;
            double totalCost = 0.0;
            var random = new Random();
            while (itemCnt < 11)
            {
                RfidTag rfidTag = basket[itemCnt];

                string itemJson = JsonSerializer.Serialize(rfidTag);
                var message = new Microsoft.Azure.ServiceBus.Message(Encoding.UTF8.GetBytes(itemJson))
                {
                    Label = "RFID_Demo",
                    //Required to find duplicate message detection.
                    MessageId = rfidTag.TagId
                };
                await queueClient.SendAsync(message);
                Console.WriteLine($"Sent: {rfidTag.Product} ");

                //Duplicate logic - When following condition will be true, it will cause same item to be sent multiple times.
                //Increment number, This will cause code to send same product message multiple times when random value will be <= 0.3                
                if (random.NextDouble() > 0.4)
                    itemCnt++;

                sentCnt++;
                totalCost += rfidTag.Price;
                Thread.Sleep(100);
            }
            Console.WriteLine($"Total {sentCnt} items scanned by RFID and total cost is {totalCost} (Including Duplicates)");
        }

        private static async Task ManageQueue(ServiceBusConfig serviceBusConfig)
        {
            var managementClient = new ManagementClient(serviceBusConfig.ConnectionString);

            //Delete the queue if exists
            if (await managementClient.QueueExistsAsync(_queueName))
                await managementClient.DeleteQueueAsync(_queueName);

            QueueDescription queueDescription = new QueueDescription(_queueName)
            {
                RequiresDuplicateDetection = true,
                DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(10)
            };

            await managementClient.CreateQueueAsync(queueDescription);
        }
    }
}
