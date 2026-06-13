using System;
using System.Collections.Generic;
using System.Text;

namespace Eventbox.Payment.Core.Enums
{
    public enum PaymentStatus
    {
        Pending,
        Succeeded,
        Failed,
        Cancelled
    }
}
