using Eventbox.Shared.Outbox;

namespace Eventbox.Payment.Core.Abstractions
{
    public interface IOutbox
    {
        Task AddOutboxMessageAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken);

        Task UpdateOutboxMessageAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken);
    }
}
