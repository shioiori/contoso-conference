using Eventbox.EventBus.Events;

namespace Eventbox.Contracts.IntegrationEvents;

public class TicketTypeDeletedIntegrationEvent : IntegrationEvent
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
}
