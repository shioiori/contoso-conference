using Eventbox.EventBus.Events;

namespace Eventbox.Contracts.IntegrationEvents;

public sealed class CheckInCompletedIntegrationEvent : IntegrationEvent
{
    public Guid TicketId { get; init; }
    public Guid EventId { get; init; }
    public Guid TicketTypeId { get; init; }
    public Guid? OperatorUserId { get; init; }
    public DateTimeOffset CheckedInAt { get; init; }
    public bool WasOverride { get; init; }
}
