using Eventbox.Shared.Outbox;

namespace Eventbox.EventBus.Core.Abstractions;

public interface IDeadLetterPublisher
{
    Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default);
}
