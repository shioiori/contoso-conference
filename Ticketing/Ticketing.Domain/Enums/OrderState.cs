using System;
using System.Collections.Generic;
using System.Text;

namespace Eventbox.TicketingDomain.Enums
{
    public enum OrderState
    {
        Pending,
        Confirmed,
        Cancelled,
        Expired
    }
}
