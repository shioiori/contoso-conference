using Eventbox.EventBus.Events;

namespace Eventbox.Contracts.IntegrationEvents;

public class EventUnpublishedEvent : IntegrationEvent
{
    public Guid EventId { get; set; }
}
