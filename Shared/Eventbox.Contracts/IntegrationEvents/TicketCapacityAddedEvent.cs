using Eventbox.EventBus.Events;

namespace Eventbox.Contracts.IntegrationEvents;

public class TicketCapacityAddedEvent : IntegrationEvent
{
    public int Id { get; set; }
    public Guid EventId { get; set; }
    public int PreviousQuantity { get; set; }
    public int NewQuantity { get; set; }
    public int AddedQuantity { get; set; }
}
