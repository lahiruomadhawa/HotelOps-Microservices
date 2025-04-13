using HotelServices.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelServices.Core.Interfaces
{
    public interface IOrderRepository
    {
        Task<bool> SaveOrderAsync(Order order);
        Task<Order> GetOrderByIdAsync(Guid id);
        Task<List<Order>> GetRecentOrdersAsync(int count = 10);
        Task<bool> UpdateOrderStatusAsync(Guid id, string status);
    }
}
