using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelServices.Core.Models
{
    public class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Type { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Payload { get; set; }
    }
}
