using Eventbox.EventBus.Events;

namespace Eventbox.Contracts.IntegrationEvents;

public class TicketCapacityAddedIntegrationEvent : IntegrationEvent
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public int NewQuantity { get; set; }
}
