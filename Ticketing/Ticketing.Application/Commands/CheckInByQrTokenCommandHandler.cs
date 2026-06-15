using Eventbox.TicketingApplication.Abstractions;
using Eventbox.TicketingApplication.Abstractions.Repositories;
using Eventbox.TicketingApplication.Dtos;
using Eventbox.TicketingApplication.Mappings;
using Eventbox.TicketingDomain.Entities.CheckInAggregate;
using Eventbox.TicketingDomain.Enums;
using MediatR;

namespace Eventbox.TicketingApplication.Commands;

public class CheckInByQrTokenCommandHandler(
    ICheckInPassRepository checkInPassRepository,
    ICheckInAttemptRepository checkInAttemptRepository,
    IRegistrationUnitOfWork unitOfWork,
    IQrTokenHasher qrTokenHasher) : IRequestHandler<CheckInByQrTokenCommand, CheckInResultDto>
{
    public async Task<CheckInResultDto> Handle(CheckInByQrTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.QrToken))
            return new CheckInResultDto(CheckInAttemptResult.InvalidToken, "QR token is required.", null);

        var qrTokenHash = qrTokenHasher.Hash(request.QrToken);
        CheckInResultDto result = null!;

        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var pass = await checkInPassRepository.GetByQrTokenHashAsync(qrTokenHash, cancellationToken);
            result = await BuildResultAndRecordAttemptAsync(request, qrTokenHash, pass, cancellationToken);
        }, cancellationToken);

        return result;
    }

    private async Task<CheckInResultDto> BuildResultAndRecordAttemptAsync(
        CheckInByQrTokenCommand request,
        string qrTokenHash,
        CheckInPass? pass,
        CancellationToken cancellationToken)
    {
        var scannedAt = DateTimeOffset.UtcNow;
        CheckInAttemptResult attemptResult;
        string message;

        if (pass is null)
        {
            attemptResult = CheckInAttemptResult.InvalidToken;
            message = "QR token is invalid.";
        }
        else if (pass.EventId != request.EventId)
        {
            attemptResult = CheckInAttemptResult.WrongEvent;
            message = "QR token does not belong to this event.";
        }
        else if (pass.Status == CheckInPassStatus.Cancelled)
        {
            attemptResult = CheckInAttemptResult.Cancelled;
            message = "Check-in pass has been cancelled.";
        }
        else if (!pass.CheckIn(request.StaffUserId ?? Guid.Empty, scannedAt))
        {
            attemptResult = CheckInAttemptResult.AlreadyCheckedIn;
            message = "Check-in pass was already used.";
        }
        else
        {
            checkInPassRepository.Update(pass);
            attemptResult = CheckInAttemptResult.Success;
            message = "Check-in completed.";
        }

        await checkInAttemptRepository.AddAsync(
            new CheckInAttempt(
                request.EventId,
                pass?.Id,
                request.StaffUserId,
                qrTokenHash,
                attemptResult,
                attemptResult == CheckInAttemptResult.Success ? null : message,
                scannedAt),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CheckInResultDto(attemptResult, message, pass?.ToDto());
    }
}
