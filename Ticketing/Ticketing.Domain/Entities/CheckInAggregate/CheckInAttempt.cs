using Eventbox.TicketingDomain.Enums;
using Eventbox.TicketingDomain.SeedWork;

namespace Eventbox.TicketingDomain.Entities.CheckInAggregate;

public class CheckInAttempt : Entity<Guid>
{
    private CheckInAttempt()
    {
        QrTokenHash = null!;
    }

    public CheckInAttempt(
        Guid eventId,
        Guid? checkInPassId,
        Guid? staffUserId,
        string qrTokenHash,
        CheckInAttemptResult result,
        string? failureReason,
        DateTimeOffset scannedAt)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("Event id is required.", nameof(eventId));

        if (string.IsNullOrWhiteSpace(qrTokenHash))
            throw new ArgumentException("QR token hash is required.", nameof(qrTokenHash));

        Id = Guid.NewGuid();
        EventId = eventId;
        CheckInPassId = checkInPassId;
        StaffUserId = staffUserId;
        QrTokenHash = qrTokenHash;
        Result = result;
        FailureReason = failureReason;
        ScannedAt = scannedAt;
    }

    public Guid EventId { get; private set; }
    public Guid? CheckInPassId { get; private set; }
    public Guid? StaffUserId { get; private set; }
    public string QrTokenHash { get; private set; }
    public CheckInAttemptResult Result { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTimeOffset ScannedAt { get; private set; }
}
