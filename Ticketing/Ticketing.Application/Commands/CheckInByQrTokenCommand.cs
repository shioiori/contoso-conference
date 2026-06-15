using Eventbox.TicketingApplication.Dtos;
using MediatR;

namespace Eventbox.TicketingApplication.Commands;

public class CheckInByQrTokenCommand : IRequest<CheckInResultDto>
{
    public Guid EventId { get; init; }
    public string QrToken { get; init; } = string.Empty;
    public Guid? StaffUserId { get; init; }
}
