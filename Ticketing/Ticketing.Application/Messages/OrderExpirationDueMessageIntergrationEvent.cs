using Eventbox.EventBus.Core.Abstractions;

namespace Eventbox.Ticketing.Application.Messages;

public sealed class OrderExpirationDueMessageIntergrationEvent : IIntegrationEvent
{
    public Guid OrderId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public Guid IntegrationEventId { get; set; } = Guid.NewGuid();
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}
