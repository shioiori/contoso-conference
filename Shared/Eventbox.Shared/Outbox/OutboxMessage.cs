using System;
using System.Collections.Generic;
using System.Text;

namespace Eventbox.Shared.Outbox
{
    public class OutboxMessage
    {
        public const int MaxRetries = 3;

        public Guid Id { get; set; }

        public string IntegrationEventType { get; set; } = default!;
        public string Content { get; set; } = default!;

        public DateTime OccurredOnUtc { get; set; }
        public DateTime? ProcessedOnUtc { get; set; }

        public string? Error { get; set; }
        public ProcessStatus Status { get; set; }
        public int RetryCount { get; set; }
    }
}
