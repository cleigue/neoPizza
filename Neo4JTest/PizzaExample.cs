namespace Neo4JTest
{
    using Neo4j.Driver.V1;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class PizzaExample : IDisposable
    {
        private readonly IDriver _driver;

        public PizzaExample(string uri, string user, string password)
        {
            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        }

        public void PopulatePizzaBusiness()
        {
            using (var session = _driver.Session())
            {
                var restaurants = session.WriteTransaction(tx =>
                {
                    var parameters = new Dictionary<string, object> {
                        { "pizza1", "Pizzeria Cozzolisi" },
                        { "pizza2", "Pizzeria Ellis" },
                        { "telephone1", "42414140" },
                        { "telephone2", "45588440" }};

                    var result = tx.Run("create (p1: Restaurant{name: $pizza1, telephone: $telephone1}), " +
                        "(p2: Restaurant{name: $pizza2, telephone: $telephone2}) " +
                        "return p1.name as pizza1, p2.name as pizza2", parameters);

                    var record = result.Single();
                    
                    return "Created: " + record["pizza1"].As<string>() + ", " + record["pizza2"].As<string>();
                });

                Console.WriteLine(restaurants);
            }
        }

        public void PopulatePizzaShopMenu()
        {
            var result = this.RunQuery("match (r:Restaurant{name: 'Pizzeria Cozzolisi'}) " +
                        "create (r)-[:SELLS{size: '6', price: '60'}]->(p:Pizza{name: 'Vegetariana'}) WITH r, p  " +
                        "create (r)-[:SELLS{size: '8', price: '80'}]->(p) WITH r, p  " +
                        "create (r)-[:SELLS{size: '12', price: '120'}]->(p) WITH r " +
                        "create (r)-[:SELLS{size: '6', price: '80'}]->(p1:Pizza{name: 'Margarita'})");

            Console.WriteLine("Menu items 1: " + result);

            result = this.RunQuery("match(r: Restaurant{ name: 'Pizzeria Ellis'}) " +
                        "create (r)-[:SELLS{size: '6', price: '80'}]->(p:Pizza{name: 'Ellis'}) WITH r, p  " +
                        "create (r)-[:SELLS{size: '8', price: '130'}]->(p) WITH r " +
                        "create (r)-[:SELLS{size: '8', price: '90'}]->(p1:Pizza{name: 'Carnivora'}) WITH r, p1  " +
                        "create (r)-[:SELLS{size: '10', price: '150'}]->(p1) ");

            Console.WriteLine("Menu items 2: " + result);            
        }

        public void CreateClient()
        {
            using (var session = _driver.Session())
            {
                var client = session.WriteTransaction(tx =>
                {
                    var result = tx.Run("create (c:Client{name: 'Garfield'})-[:REGISTERED_IN]->(a:Address{street: 'America', number: 'O2231'})," +
                        "(c)-[:REGISTERED_IN]->(a2:Address{street: 'Melchor Perez', number: 'O1181'}) RETURN c.name as clientName");

                    var record = result.Single();

                    return "Registered client: " + record["clientName"].As<string>();
                });

                Console.WriteLine(client);
            }
        }

        public void OrderPizza()
        {
            var result = this.RunQuery("match (c:Client{name: 'Garfield'}), (p:Pizza{name: 'Ellis'}), (p1:Pizza{name: 'Carnivora'})," +
                        "(a:Address{number: 'O2231'}) " +
                        "create (c)-[:ORDERED]->(o:Order{createdAt: TIMESTAMP(), number : '00141'})," +
                        "(o)-[:CONTAINS{quantity: 1, totalPrice: 150}]->(p), (o)-[:CONTAINS{quantity: 1, totalPrice: 80}]->(p1)," +
                        "(o)-[:DELIVER_TO]->(a)");

            Console.WriteLine("Order 1 placed: " + result);

            result = this.RunQuery("match (c:Client{name: 'Garfield'}), (p:Pizza{name: 'Vegetariana'}), (p1:Pizza{name: 'Margarita'})," +
                        "(a:Address{number: 'O1181'}) " +
                        "create (c)-[:ORDERED]->(o:Order{createdAt: TIMESTAMP(), number : '00123'})," +
                        "(o)-[:CONTAINS{quantity: 2, totalPrice: 300}]->(p), (o)-[:CONTAINS{quantity: 1, totalPrice: 70, discount: 20}]->(p1)," +
                        "(o)-[:DELIVER_TO]->(a)");

            Console.WriteLine("Order 2 placed: " + result);
        }

        private string RunQuery(string queryString)
        {
            using (var session = _driver.Session())
            {
                var queryResult = session.WriteTransaction(tx =>
                {
                    var result = tx.Run(queryString);

                    var record = result.Summary.Counters.ToString();

                    return record;
                });

                return queryResult;
            }
        }


        public void Dispose()
        {
            _driver?.Dispose();
        }

    }
}
