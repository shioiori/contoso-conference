using Eventbox.TicketingDomain.Enums;

namespace Eventbox.TicketingApplication.Dtos;

public class TicketDto
{
    public Guid Id { get; init; }
    public int TicketTypeId { get; init; }
    public int SequenceNumber { get; init; }
    public TicketState TicketState { get; init; }
    public string? QrToken { get; init; }
    public CheckInPassStatus? CheckInStatus { get; init; }
    public DateTimeOffset? CheckedInAt { get; init; }
}
