using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelServices.Infrastructure.Configuration
{
    public class ConnectionStrings
    {
        public string PostgreSQL { get; set; }
        public string Redis { get; set; }
        public string RabbitMQ { get; set; }
    }
}
