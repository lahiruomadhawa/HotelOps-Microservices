using System;
using System.Text.Json;
using System.Threading.Tasks;
using HotelServices.Core.Models;
using HotelServices.Core.Services;

namespace HotelServices.Infrastructure.Messaging
{
    public class MessageProcessor
    {
        private readonly IOrderService _orderService;

        public MessageProcessor(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task ProcessMessageAsync(Message message)
        {
            Console.WriteLine($"Processing message: {message.Type} from {message.Source}");

            try
            {
                switch (message.Type)
                {
                    case "OrderVerification":
                        var orderResponse = JsonSerializer.Deserialize<Order>(message.Payload);
                        await _orderService.VerifyOrderAsync(orderResponse);
                        break;

                    case "OrderStatusUpdate":
                        var orderStatus = JsonSerializer.Deserialize<Order>(message.Payload);
                        Console.WriteLine($"Order {orderStatus.Id} status updated to: {orderStatus.Status}");
                        break;

                    default:
                        Console.WriteLine($"Unknown message type: {message.Type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        }
    }
}
