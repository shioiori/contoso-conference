using System;
using System.Collections.Generic;
using System.Text;

namespace Eventbox.Ticketing.Application.Abstractions.Jobs
{
    public interface IOrderExpirationReconciliationJob
    {
        Task RunAsync(CancellationToken cancellationToken = default);
    }
}
