using Broker.Models;
using Newtonsoft.Json;
using System.Text;
using RabbitMQ.Client;

namespace Broker.Infrastructure
{
    public class MessageHelper
    {
        private readonly IChannel _channel;

        public MessageHelper(IChannel channel)
        {
            _channel = channel;
        }

        public async Task SendDirectMessage(string routingKey, Message message)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            await _channel.BasicPublishAsync<BasicProperties>(exchange: "hotel.direct",
                                routingKey: routingKey,
                                basicProperties: null,
                                mandatory: false,
                                body: body);
        }

        public async Task SendTopicMessage(string routingKey, Message message)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            await _channel.BasicPublishAsync<BasicProperties>(exchange: "hotel.topic",
                                routingKey: routingKey,
                                basicProperties: null,
                                mandatory:false,
                                body: body);
        }

        public async Task SendBroadcast(Message message)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            await _channel.BasicPublishAsync<BasicProperties>(exchange: "hotel.fanout",
                                routingKey: "",
                                basicProperties: null,
                                mandatory: false,
                                body: body);
        }
    }
}
