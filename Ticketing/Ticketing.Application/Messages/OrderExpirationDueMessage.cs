using Eventbox.EventBus.Core.Abstractions;

namespace Eventbox.Ticketing.Application.Messages;

public sealed class OrderExpirationDueMessage : IIntegrationEvent
{
    public OrderExpirationDueMessage(Guid orderId, DateTimeOffset expiresAt)
    {
        OrderId = orderId;
        ExpiresAt = expiresAt;
    }

    public Guid OrderId { get; }
    public DateTimeOffset ExpiresAt { get; }
    public Guid IntegrationEventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}
