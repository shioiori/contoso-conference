using Eventbox.Ticketing.Application.Dtos.CheckIn;
using MediatR;

namespace Eventbox.Ticketing.Application.Commands.CheckIn;

public class CheckInByQrTokenCommand : IRequest<CheckInResultDto>
{
    public Guid EventId { get; init; }
    public string QrToken { get; init; } = string.Empty;
    public Guid? StaffUserId { get; init; }
}
