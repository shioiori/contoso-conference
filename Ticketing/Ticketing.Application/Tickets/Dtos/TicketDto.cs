using Eventbox.Ticketing.Domain.Enums;

namespace Eventbox.Ticketing.Application.Dtos;

public class TicketDto
{
    public Guid Id { get; init; }
    public Guid EventId { get; init; }
    public Guid TicketTypeId { get; init; }
    public int SequenceNumber { get; init; }
    public TicketState TicketState { get; init; }
    public string? QrToken { get; init; }
    public CheckInStatus? CheckInStatus { get; init; }
    public DateTimeOffset? CheckedInAt { get; init; }
    public Guid? CheckedInByUserId { get; init; }
}
