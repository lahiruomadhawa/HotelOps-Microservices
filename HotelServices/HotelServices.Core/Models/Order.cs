using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelServices.Core.Models
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int RoomNumber { get; set; }
        public string GuestName { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public decimal TotalAmount { get; set; }
        public DateTime OrderTime { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending";
    }
}
