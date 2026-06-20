using Eventbox.EventBus.Core.Abstractions;

namespace Eventbox.Contracts.IntegrationEvents;

public sealed class OrderExpirationDueMessageIntergrationEvent : IIntegrationEvent
{
    public Guid OrderId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public Guid IntegrationEventId { get; set; } = Guid.NewGuid();
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}
