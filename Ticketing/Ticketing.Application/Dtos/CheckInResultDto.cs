using Eventbox.TicketingDomain.Enums;

namespace Eventbox.TicketingApplication.Dtos;

public record CheckInResultDto(
    CheckInAttemptResult Result,
    string Message,
    TicketDto? Ticket);
