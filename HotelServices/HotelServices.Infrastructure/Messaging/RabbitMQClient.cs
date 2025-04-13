using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HotelServices.Core.Interfaces;
using HotelServices.Core.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HotelServices.Infrastructure.Messaging
{
    public class RabbitMQClient : IMessageBroker, IDisposable
    {
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private bool _disposed = false;

        public RabbitMQClient(string hostName)
        {
            try
            {
                _factory = new ConnectionFactory() { HostName = hostName };
                _connection = _factory.CreateConnectionAsync();
                _channel = _connection.CreateChannelAsync();

                // Declare exchange
                _channel.ExchangeDeclareAsync(exchange: "hotel.direct", type: ExchangeType.Direct);

                // Declare queues
                _channel.QueueDeclareAsync(queue: "registry.queue", durable: true, exclusive: false, autoDelete: false);
                _channel.QueueDeclareAsync(queue: "kitchen.queue", durable: true, exclusive: false, autoDelete: false);

                // Bind queues to exchange
                _channel.QueueBindAsync(queue: "registry.queue", exchange: "hotel.direct", routingKey: "registry");
                _channel.QueueBindAsync(queue: "kitchen.queue", exchange: "hotel.direct", routingKey: "kitchen");

                Console.WriteLine("RabbitMQ initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RabbitMQ initialization failed: {ex.Message}");
                throw;
            }
        }

        public Task PublishMessageAsync(Message message, string routingKey)
        {
            try
            {
                var messageJson = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(messageJson);

                _channel.BasicPublishAsync<BasicProperties>(exchange: "hotel.direct",
                                    routingKey: routingKey,
                                    basicProperties: null,
                                    mandatory: false,
                                    body: body);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing message: {ex.Message}");
                throw;
            }
        }

        public void SubscribeToQueue(string queueName, Func<Message, Task> messageHandler)
        {
            try
            {
                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);

                    try
                    {
                        var message = JsonSerializer.Deserialize<Message>(messageJson);
                        await messageHandler(message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing received message: {ex.Message}");
                    }
                };

                _channel.BasicConsumeAsync(queue: queueName,
                                    autoAck: true,
                                    consumer: consumer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error subscribing to queue: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _channel?.CloseAsync();
                    _connection?.CloseAsync();
                }

                _disposed = true;
            }
        }
    }
}
