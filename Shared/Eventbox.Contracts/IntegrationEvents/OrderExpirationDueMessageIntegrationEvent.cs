using Eventbox.EventBus.Events;

namespace Eventbox.Contracts.IntegrationEvents;

public sealed class OrderExpirationDueMessageIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
