using Eventbox.EventBus.Events;

namespace Eventbox.Contracts.IntegrationEvents;

public class EventUpdatedEvent : IntegrationEvent
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateTimeOffset From { get; set; }
    public DateTimeOffset To { get; set; }
    public bool IsPublished { get; set; }
}
