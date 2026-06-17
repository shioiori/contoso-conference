using Eventbox.Ticketing.Application.Dtos;
using MediatR;

namespace Eventbox.Ticketing.Application.Commands;

public class CheckInByQrTokenCommand : IRequest<CheckInResultDto>
{
    public Guid EventId { get; init; }
    public string QrToken { get; init; } = string.Empty;
    public Guid? StaffUserId { get; init; }
}
