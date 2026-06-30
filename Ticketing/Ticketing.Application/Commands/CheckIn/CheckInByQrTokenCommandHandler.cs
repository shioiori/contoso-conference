using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Application.Dtos.CheckIn;
using Eventbox.Ticketing.Application.Dtos.Tickets;
using Eventbox.Ticketing.Domain.Enums;
using Mapster;
using MediatR;

namespace Eventbox.Ticketing.Application.Commands.CheckIn;

public class CheckInByQrTokenCommandHandler(
    ITicketRepository ticketRepository,
    IEventSnapshotRepository eventSnapshotRepository,
    IQrTokenHasher qrTokenHasher,
    TimeProvider timeProvider) : IRequestHandler<CheckInByQrTokenCommand, CheckInResultDto>
{
    public async Task<CheckInResultDto> Handle(CheckInByQrTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.QrToken))
            return new CheckInResultDto{
                Result = CheckInAttemptResult.InvalidToken,
                Message = "QR token is required."
            };

        var qrTokenHash = qrTokenHasher.Hash(request.QrToken);
        var ticket = await ticketRepository.GetByQrTokenHashAsync(qrTokenHash, cancellationToken);
        var now = timeProvider.GetUtcNow();

        if (ticket is null)
            return new CheckInResultDto{
                Result = CheckInAttemptResult.InvalidToken,
                Message = "QR token is invalid."
            };

        if (ticket.EventId != request.EventId)
            return new CheckInResultDto{
                Result = CheckInAttemptResult.WrongEvent,
                Message = "QR token does not belong to this event.",
                Ticket = ticket.Adapt<TicketDto>()
            };

        if (ticket.TicketState == TicketState.Cancelled)
            return new CheckInResultDto{
                Result = CheckInAttemptResult.Cancelled,
                Message = "Ticket has been cancelled.",
                Ticket = ticket.Adapt<TicketDto>()
            };

        var snapshot = await eventSnapshotRepository.GetByEventIdAsync(request.EventId, cancellationToken);
        if (snapshot is not null && !snapshot.IsCheckInAvailable(now))
            return new CheckInResultDto{
                Result = CheckInAttemptResult.CheckInUnavailable,
                Message = "Check-in is not available for this event time window.",
                Ticket = ticket.Adapt<TicketDto>()
            };

        if (ticket.CheckedInAt.HasValue)
            return new CheckInResultDto{
                Result = CheckInAttemptResult.AlreadyCheckedIn,
                Message = "Ticket was already checked in.",
                Ticket = ticket.Adapt<TicketDto>()
            };

        var updated = await ticketRepository.TryMarkCheckedInAsync(ticket.Id, request.StaffUserId, now, cancellationToken);
        ticket = await ticketRepository.GetByQrTokenHashAsync(qrTokenHash, cancellationToken);

        if (updated == 0)
            return new CheckInResultDto{
                Result = CheckInAttemptResult.AlreadyCheckedIn,
                Message = "Ticket was already checked in.",
                Ticket = ticket?.Adapt<TicketDto>()
            };

        return new CheckInResultDto{
            Result = CheckInAttemptResult.Success,
            Message = "Check-in completed.",
            Ticket = ticket?.Adapt<TicketDto>()
        };
    }
}
