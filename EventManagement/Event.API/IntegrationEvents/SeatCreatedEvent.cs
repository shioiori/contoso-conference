using Eventbox.EventBus.Events;

namespace Eventbox.EventManagement.EventApi.IntegrationEvents;

public class SeatCreatedEvent : IntegrationEvent
{
    public int SeatTypeId { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = "";
    public int Quota { get; set; }
}
