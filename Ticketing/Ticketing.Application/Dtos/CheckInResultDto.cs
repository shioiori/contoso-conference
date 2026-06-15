using Eventbox.Ticketing.Domain.Enums;

namespace Eventbox.Ticketing.Application.Dtos;

public record CheckInResultDto(
    CheckInAttemptResult Result,
    string Message,
    TicketDto? Ticket);
