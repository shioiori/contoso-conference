using Eventbox.TicketingApplication.Abstractions;
using Eventbox.TicketingApplication.Abstractions.Repositories;
using Eventbox.TicketingApplication.Dtos;
using Eventbox.TicketingDomain.Enums;
using Mapster;
using MediatR;

namespace Eventbox.TicketingApplication.Commands;

public class CheckInByQrTokenCommandHandler(
    IOrderRepository orderRepository,
    IEventScheduleRepository eventScheduleRepository,
    IQrTokenHasher qrTokenHasher) : IRequestHandler<CheckInByQrTokenCommand, CheckInResultDto>
{
    public async Task<CheckInResultDto> Handle(CheckInByQrTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.QrToken))
            return new CheckInResultDto(CheckInAttemptResult.InvalidToken, "QR token is required.", null);

        var qrTokenHash = qrTokenHasher.Hash(request.QrToken);
        var ticket = await orderRepository.GetTicketByQrTokenHashAsync(qrTokenHash, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        if (ticket is null)
            return new CheckInResultDto(CheckInAttemptResult.InvalidToken, "QR token is invalid.", null);

        if (ticket.EventId != request.EventId)
            return new CheckInResultDto(CheckInAttemptResult.WrongEvent, "QR token does not belong to this event.", ticket.Adapt<TicketDto>());

        if (ticket.TicketState == TicketState.Cancelled)
            return new CheckInResultDto(CheckInAttemptResult.Cancelled, "Ticket has been cancelled.", ticket.Adapt<TicketDto>());

        var schedule = await eventScheduleRepository.GetByEventIdAsync(request.EventId, cancellationToken);
        if (schedule is not null && !schedule.IsCheckInAvailable(now))
            return new CheckInResultDto(CheckInAttemptResult.CheckInUnavailable, "Check-in is not available for this event time window.", ticket.Adapt<TicketDto>());

        if (ticket.CheckedInAt.HasValue)
            return new CheckInResultDto(CheckInAttemptResult.AlreadyCheckedIn, "Ticket was already checked in.", ticket.Adapt<TicketDto>());

        var updated = await orderRepository.TryMarkTicketCheckedInAsync(ticket.Id, request.StaffUserId, now, cancellationToken);
        ticket = await orderRepository.GetTicketByQrTokenHashAsync(qrTokenHash, cancellationToken) ?? ticket;

        if (updated == 0)
            return new CheckInResultDto(CheckInAttemptResult.AlreadyCheckedIn, "Ticket was already checked in.", ticket.Adapt<TicketDto>());

        return new CheckInResultDto(CheckInAttemptResult.Success, "Check-in completed.", ticket.Adapt<TicketDto>());
    }
}
