using Eventbox.EventBus.Events;

namespace Eventbox.EventManagement.EventApi.IntegrationEvents;

public class EventCreatedEvent : IntegrationEvent
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Description { get; set; }
    public DateTimeOffset From { get; set; }
    public DateTimeOffset To { get; set; }
    public string AccessCode { get; set; } = "";
}
