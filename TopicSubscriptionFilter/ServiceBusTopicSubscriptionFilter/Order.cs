using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceBusTopicSubscriptionFilter
{
    public class Order
    {
        public string Name { get; set; }

        public DateTime OrderDate { get; set; }

        public int Items { get; set; }

        public double Value { get; set; }

        public string Priority { get; set; }

        public string Region { get; set; }

        public bool HasLoyltyCard { get; set; }

        public override string ToString()
        {
            return $"{ Name }\tItm:{ Items }\t${ Value }\t{ Region }\tLoyal:{ HasLoyltyCard }";
        }

        public static IEnumerable<Order> CreateOrders()
        {
            var orders = new List<Order>
            {
                new Order()
                {
                    Name = "Loyal Customer",
                    Value = 19.99,
                    Region = "USA",
                    Items = 1,
                    HasLoyltyCard = true
                },
                new Order()
                {
                    Name = "Large Order",
                    Value = 49.99,
                    Region = "USA",
                    Items = 50,
                    HasLoyltyCard = false
                },
                new Order()
                {
                    Name = "High Value",
                    Value = 749.45,
                    Region = "USA",
                    Items = 45,
                    HasLoyltyCard = false
                },
                new Order()
                {
                    Name = "Loyal Europe",
                    Value = 49.45,
                    Region = "EU",
                    Items = 3,
                    HasLoyltyCard = true
                },
                new Order()
                {
                    Name = "UK Order",
                    Value = 49.45,
                    Region = "UK",
                    Items = 3,
                    HasLoyltyCard = false
                }
            };

            return orders;
        }
    }
}
