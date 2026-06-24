using Eventbox.Ticketing.Application.Dtos.Tickets;
using Eventbox.Ticketing.Domain.Enums;

namespace Eventbox.Ticketing.Application.Dtos.CheckIn;

public class CheckInResultDto
{
    public CheckInAttemptResult Result { get; set; }
    public string Message { get; set; }
    public TicketDto Ticket { get; set; }
}