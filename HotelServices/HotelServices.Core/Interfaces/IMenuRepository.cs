using HotelServices.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelServices.Core.Interfaces
{
    public interface IMenuRepository
    {
        Task<List<MenuItem>> GetAllAvailableMenuItemsAsync();
        Task<MenuItem> GetMenuItemByIdAsync(int id);
        Task<bool> UpdateMenuItemAvailabilityAsync(int id, bool available);
    }
}
