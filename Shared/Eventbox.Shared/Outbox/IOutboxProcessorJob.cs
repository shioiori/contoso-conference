using System;
using System.Collections.Generic;
using System.Text;

namespace Eventbox.Shared.Outbox
{
    public interface IOutboxProcessorJob
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}
