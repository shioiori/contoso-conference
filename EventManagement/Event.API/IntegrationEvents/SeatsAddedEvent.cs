using Eventbox.EventBus.Events;

namespace Eventbox.EventManagement.EventApi.IntegrationEvents;

public class SeatsAddedEvent : IntegrationEvent
{
    public int SeatTypeId { get; set; }
    public Guid EventId { get; set; }
    public int PreviousQuota { get; set; }
    public int NewQuota { get; set; }
    public int AddedQuantity { get; set; }
}
