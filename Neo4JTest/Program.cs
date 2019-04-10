using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Neo4JTest
{
    class Program
    {
        private static IConfiguration config;

        public static void Main()
        {
            config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();

             //HelloWorldTest();

            PizzaOrder();
        }

        public static void HelloWorldTest()
        {
            var neo4jServer = config["neo4jServer"];
            var neo4jBoltPort = config["neo4jBoltPort"];

            using (var greeter = new HelloWorldExample($"bolt://{neo4jServer}:{neo4jBoltPort}", config["neo4jUser"], config["neo4jPassword"]))
            {
                greeter.PrintGreeting("hello, world");
            }
        }

        public static void PizzaOrder()
        {
            var neo4jServer = config["neo4jServer"];
            var neo4jBoltPort = config["neo4jBoltPort"];

            using (var pizzaShop = new PizzaExample($"bolt://{neo4jServer}:{neo4jBoltPort}", config["neo4jUser"], config["neo4jPassword"]))
            {
                pizzaShop.PopulatePizzaBusiness();
                pizzaShop.PopulatePizzaShopMenu();
                pizzaShop.CreateClient();
                pizzaShop.OrderPizza();
            }
        }
    }
}
