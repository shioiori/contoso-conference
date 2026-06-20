using Eventbox.EventBus.Events;

namespace Eventbox.Contracts.IntegrationEvents;

public class EventPublishedEvent : IntegrationEvent
{
    public Guid EventId { get; set; }
}
