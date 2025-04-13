using RabbitMQ.Client;

namespace Broker
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Message Broker Service");
            Console.WriteLine("---------------------");
            Console.WriteLine("Setting up RabbitMQ exchanges and queues...");

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = await factory.CreateConnectionAsync())
            using (var channel = await connection.CreateChannelAsync())
            {
                // Declare exchanges
                await channel.ExchangeDeclareAsync(exchange: "hotel.direct", type: ExchangeType.Direct);
                await channel.ExchangeDeclareAsync(exchange: "hotel.topic", type: ExchangeType.Topic);
                await channel.ExchangeDeclareAsync(exchange: "hotel.fanout", type: ExchangeType.Fanout);

                // Declare queues for each service
                await channel.QueueDeclareAsync(queue: "registry.queue", durable: true, exclusive: false, autoDelete: false);
                await channel.QueueDeclareAsync(queue: "kitchen.queue", durable: true, exclusive: false, autoDelete: false);
                await channel.QueueDeclareAsync(queue: "housekeeping.queue", durable: true, exclusive: false, autoDelete: false);
                await channel.QueueDeclareAsync(queue: "reservation.queue", durable: true, exclusive: false, autoDelete: false);

                // Bind queues to exchanges
                await channel.QueueBindAsync(queue: "registry.queue", exchange: "hotel.direct", routingKey: "registry");
                await channel.QueueBindAsync(queue: "kitchen.queue", exchange: "hotel.direct", routingKey: "kitchen");
                await channel.QueueBindAsync(queue: "housekeeping.queue", exchange: "hotel.direct", routingKey: "housekeeping");
                await channel.QueueBindAsync(queue: "reservation.queue", exchange: "hotel.direct", routingKey: "reservation");

                // Topic bindings for event-based communication
                await channel.QueueBindAsync(queue: "registry.queue", exchange: "hotel.topic", routingKey: "order.#");
                await channel.QueueBindAsync(queue: "kitchen.queue", exchange: "hotel.topic", routingKey: "kitchen.#");
                await channel.QueueBindAsync(queue: "housekeeping.queue", exchange: "hotel.topic", routingKey: "housekeeping.#");
                await channel.QueueBindAsync(queue: "reservation.queue", exchange: "hotel.topic", routingKey: "reservation.#");

                // Fanout bindings for broadcasts
                await channel.QueueBindAsync(queue: "registry.queue", exchange: "hotel.fanout", routingKey: "");
                await channel.QueueBindAsync(queue: "kitchen.queue", exchange: "hotel.fanout", routingKey: "");
                await channel.QueueBindAsync(queue: "housekeeping.queue", exchange: "hotel.fanout", routingKey: "");
                await channel.QueueBindAsync(queue: "reservation.queue", exchange: "hotel.fanout", routingKey: "");

                Console.WriteLine("RabbitMQ setup complete.");
                Console.WriteLine("Message broker is running. Press [Enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
