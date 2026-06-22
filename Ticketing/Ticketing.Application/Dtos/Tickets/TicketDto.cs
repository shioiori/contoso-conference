using Eventbox.Ticketing.Domain.Enums;

namespace Eventbox.Ticketing.Application.Dtos.Tickets;

public class TicketDto
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid TicketTypeId { get; set; }
    public int SequenceNumber { get; set; }
    public TicketState TicketState { get; set; }
    public string? QrToken { get; set; }
    public CheckInStatus? CheckInStatus { get; set; }
    public DateTimeOffset? CheckedInAt { get; set; }
    public Guid? CheckedInByUserId { get; set; }
}
