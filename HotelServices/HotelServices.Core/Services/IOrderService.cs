using HotelServices.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelServices.Core.Services
{
    public interface IOrderService
    {
        Task<bool> CreateOrderAsync(Order order);
        Task<bool> VerifyOrderAsync(Order order);
        Task<bool> SendOrderToKitchenAsync(Order order);
        Task<Order> GetOrderByIdAsync(Guid id);
        Task<List<Order>> GetRecentOrdersAsync(int count = 10);
    }
}
