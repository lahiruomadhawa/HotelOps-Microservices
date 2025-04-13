using HotelServices.Core.Interfaces;
using HotelServices.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HotelServices.Core.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMessageBroker _messageBroker;
        private readonly Dictionary<Guid, Order> _pendingOrders = new Dictionary<Guid, Order>();

        public OrderService(IOrderRepository orderRepository, IMessageBroker messageBroker)
        {
            _orderRepository = orderRepository;
            _messageBroker = messageBroker;

            // Subscribe to order verification responses
            _messageBroker.SubscribeToQueue("registry.queue", HandleIncomingMessageAsync);
        }

        public async Task<bool> CreateOrderAsync(Order order)
        {
            // Add to pending orders
            _pendingOrders[order.Id] = order;

            // Send verification request
            return await SendOrderVerificationRequestAsync(order);
        }

        public async Task<bool> VerifyOrderAsync(Order order)
        {
            if (order.Status == "Confirmed")
            {
                var result = await _orderRepository.SaveOrderAsync(order);
                if (result)
                {
                    await SendOrderToKitchenAsync(order);
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> SendOrderToKitchenAsync(Order order)
        {
            var message = new Message
            {
                Type = "OrderPreparation",
                Source = "Registry",
                Destination = "Kitchen",
                Payload = JsonSerializer.Serialize(order)
            };

            await _messageBroker.PublishMessageAsync(message, "kitchen");
            return true;
        }

        public async Task<Order> GetOrderByIdAsync(Guid id)
        {
            return await _orderRepository.GetOrderByIdAsync(id);
        }

        public async Task<List<Order>> GetRecentOrdersAsync(int count = 10)
        {
            return await _orderRepository.GetRecentOrdersAsync(count);
        }

        private async Task<bool> SendOrderVerificationRequestAsync(Order order)
        {
            var message = new Message
            {
                Type = "OrderVerificationRequest",
                Source = "Registry",
                Destination = "Kitchen",
                Payload = JsonSerializer.Serialize(order)
            };

            await _messageBroker.PublishMessageAsync(message, "kitchen");
            return true;
        }

        private async Task HandleIncomingMessageAsync(Message message)
        {
            try
            {
                switch (message.Type)
                {
                    case "OrderVerification":
                        var orderResponse = JsonSerializer.Deserialize<Order>(message.Payload);

                        if (_pendingOrders.ContainsKey(orderResponse.Id))
                        {
                            if (orderResponse.Status == "Confirmed")
                            {
                                await VerifyOrderAsync(_pendingOrders[orderResponse.Id]);
                            }

                            _pendingOrders.Remove(orderResponse.Id);
                        }
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
