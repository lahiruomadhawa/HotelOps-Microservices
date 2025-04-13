using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broker.Models
{
    // Base message class
    public class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public MessageType Type { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Payload { get; set; }
    }
}
