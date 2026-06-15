using System;
using System.Collections.Generic;
using System.Text;

namespace Eventbox.Ticketing.Domain.Enums
{
    public enum OrderState
    {
        Pending,
        Confirmed,
        Cancelled,
        Expired
    }
}
