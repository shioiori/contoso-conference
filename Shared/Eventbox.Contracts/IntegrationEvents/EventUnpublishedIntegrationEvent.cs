using Eventbox.EventBus.Events;

namespace Eventbox.Contracts.IntegrationEvents;

public class EventUnpublishedIntegrationEvent : IntegrationEvent
{
    public Guid EventId { get; set; }
}
