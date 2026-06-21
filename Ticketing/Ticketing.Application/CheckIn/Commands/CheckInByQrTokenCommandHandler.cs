using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Application.Dtos;
using Eventbox.Ticketing.Domain.Enums;
using Mapster;
using MediatR;

namespace Eventbox.Ticketing.Application.Commands;

public class CheckInByQrTokenCommandHandler(
    ITicketRepository ticketRepository,
    IEventSnapshotRepository eventSnapshotRepository,
    IQrTokenHasher qrTokenHasher) : IRequestHandler<CheckInByQrTokenCommand, CheckInResultDto>
{
    public async Task<CheckInResultDto> Handle(CheckInByQrTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.QrToken))
            return new CheckInResultDto(CheckInAttemptResult.InvalidToken, "QR token is required.", null);

        var qrTokenHash = qrTokenHasher.Hash(request.QrToken);
        var ticket = await ticketRepository.GetByQrTokenHashAsync(qrTokenHash, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        if (ticket is null)
            return new CheckInResultDto(CheckInAttemptResult.InvalidToken, "QR token is invalid.", null);

        if (ticket.EventId != request.EventId)
            return new CheckInResultDto(CheckInAttemptResult.WrongEvent, "QR token does not belong to this event.", ticket.Adapt<TicketDto>());

        if (ticket.TicketState == TicketState.Cancelled)
            return new CheckInResultDto(CheckInAttemptResult.Cancelled, "Ticket has been cancelled.", ticket.Adapt<TicketDto>());

        var snapshot = await eventSnapshotRepository.GetByEventIdAsync(request.EventId, cancellationToken);
        if (snapshot is not null && !snapshot.IsCheckInAvailable(now))
            return new CheckInResultDto(CheckInAttemptResult.CheckInUnavailable, "Check-in is not available for this event time window.", ticket.Adapt<TicketDto>());

        if (ticket.CheckedInAt.HasValue)
            return new CheckInResultDto(CheckInAttemptResult.AlreadyCheckedIn, "Ticket was already checked in.", ticket.Adapt<TicketDto>());

        var updated = await ticketRepository.TryMarkCheckedInAsync(ticket.Id, request.StaffUserId, now, cancellationToken);
        ticket = await ticketRepository.GetByQrTokenHashAsync(qrTokenHash, cancellationToken) ?? ticket;

        if (updated == 0)
            return new CheckInResultDto(CheckInAttemptResult.AlreadyCheckedIn, "Ticket was already checked in.", ticket.Adapt<TicketDto>());

        return new CheckInResultDto(CheckInAttemptResult.Success, "Check-in completed.", ticket.Adapt<TicketDto>());
    }
}
