using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broker.Models
{
    // Message types for different operations
    public enum MessageType
    {
        OrderRequest,
        OrderVerification,
        OrderConfirmation,
        OrderRejection,
        RoomReservationRequest,
        RoomReservationConfirmation,
        HousekeepingRequest,
        HousekeepingConfirmation
    }
}
