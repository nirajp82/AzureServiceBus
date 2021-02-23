using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Text;

namespace ServiceBus.Management
{
    class Program
    {
        static void Main()
        {
            ServiceBusConfig serviceBusConfig = InitServiceBusConfig();
            ManagementHelper helper = new ManagementHelper(serviceBusConfig.ConnectionString);
            bool done = false;
            do
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Please enter your command:");
                Console.Write(">> ");
                Console.ForegroundColor = ConsoleColor.Gray;
                string commandLine = Console.ReadLine();
                string[] commands = commandLine.Split(' ');
                try
                {
                    if (string.IsNullOrWhiteSpace(commands[0]) || string.Compare("exit", commands[0], true) == 0)
                    {
                        done = true;
                        return;
                    }
                    switch (commands[0].ToLower())
                    {
                        case "createqueue":
                        case "cq":
                            if (commands.Length > 1)
                                helper.CreateQueueAsync(commands[1]).Wait();
                            else
                                PrintWarning("Queue path missing after command name");
                            break;
                        case "deletequeue":
                        case "dq":
                            if (commands.Length > 1)
                                helper.DeleteQueueAsync(commands[1]).Wait();
                            else
                                PrintWarning("Queue path missing after command name");
                            break;
                        case "listqueues":
                        case "lq":
                            helper.ListQueuesAsync().Wait();
                            break;
                        case "getqueue":
                        case "gq":
                            if (commands.Length > 1)
                                helper.GetQueueAsync(commands[1]).Wait();
                            else
                                PrintWarning("Queue path missing after command name");
                            break;
                        case "createtopic":
                        case "ct":
                            if (commands.Length > 1)
                                helper.CreateTopicAsync(commands[1]).Wait();
                            else
                                PrintWarning("Topic path missing after command name");
                            break;
                        case "CreateSubscriptionAsync":
                        case "cs":
                            if (commands.Length > 2)
                                helper.CreateSubscriptionAsync(commands[1], commands[2]).Wait();
                            else
                                PrintWarning("Topic/Subscription path missing after command name");
                            break;
                        case "lts":
                            helper.ListTopicsAndSubscriptionsAsync().Wait();
                            break;
                        default:
                            PrintWarning("Invalid command, Please try again");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            } while (!done);
        }

        private static void PrintWarning(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(msg);
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
