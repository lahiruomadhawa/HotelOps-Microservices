using HotelServices.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelServices.Core.Interfaces
{
    public interface IMessageBroker
    {
        Task PublishMessageAsync(Message message, string routingKey);
        void SubscribeToQueue(string queueName, Func<Message, Task> messageHandler);
        void Dispose();
    }
}
