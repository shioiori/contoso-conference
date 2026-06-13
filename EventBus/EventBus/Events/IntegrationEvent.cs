using Eventbox.EventBus.Core.Abstractions;

namespace Eventbox.EventBus.Events;

public abstract class IntegrationEvent : IIntegrationEvent
{
    public Guid IntegrationEventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}
