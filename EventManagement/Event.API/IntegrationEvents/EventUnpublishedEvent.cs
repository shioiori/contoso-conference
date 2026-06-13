using Eventbox.EventBus.Events;

namespace Eventbox.EventManagement.EventApi.IntegrationEvents;

public class EventUnpublishedEvent : IntegrationEvent
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
}
