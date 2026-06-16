using System;
using System.Collections.Generic;
using System.Text;

namespace Eventbox.Shared.Outbox
{
    public enum ProcessStatus
    {
        Pending,
        Processing,
        Processed,
        Failed
    }
}
