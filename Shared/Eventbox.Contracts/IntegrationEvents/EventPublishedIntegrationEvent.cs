using Eventbox.EventBus.Events;

namespace Eventbox.Contracts.IntegrationEvents;

public class EventPublishedIntegrationEvent : IntegrationEvent
{
    public Guid EventId { get; set; }
}
