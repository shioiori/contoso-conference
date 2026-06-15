using Eventbox.TicketingDomain.Enums;
using Eventbox.TicketingDomain.SeedWork;

namespace Eventbox.TicketingDomain.Entities.CheckInAggregate;

public class CheckInPass : Entity<Guid>
{
    private CheckInPass()
    {
        QrToken = null!;
        QrTokenHash = null!;
        AttendeeName = null!;
        AttendeeEmail = null!;
    }

    public CheckInPass(
        Guid eventId,
        Guid registrationOrderId,
        Guid registrationTicketId,
        int ticketTypeId,
        string attendeeName,
        string attendeeEmail,
        string qrToken,
        string qrTokenHash)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("Event id is required.", nameof(eventId));

        if (registrationOrderId == Guid.Empty)
            throw new ArgumentException("Registration order id is required.", nameof(registrationOrderId));

        if (registrationTicketId == Guid.Empty)
            throw new ArgumentException("Registration ticket id is required.", nameof(registrationTicketId));

        if (ticketTypeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(ticketTypeId), "Ticket type id must be greater than zero.");

        if (string.IsNullOrWhiteSpace(attendeeName))
            throw new ArgumentException("Attendee name is required.", nameof(attendeeName));

        if (string.IsNullOrWhiteSpace(attendeeEmail))
            throw new ArgumentException("Attendee email is required.", nameof(attendeeEmail));

        if (string.IsNullOrWhiteSpace(qrToken))
            throw new ArgumentException("QR token is required.", nameof(qrToken));

        if (string.IsNullOrWhiteSpace(qrTokenHash))
            throw new ArgumentException("QR token hash is required.", nameof(qrTokenHash));

        Id = Guid.NewGuid();
        EventId = eventId;
        RegistrationOrderId = registrationOrderId;
        RegistrationTicketId = registrationTicketId;
        TicketTypeId = ticketTypeId;
        AttendeeName = attendeeName.Trim();
        AttendeeEmail = attendeeEmail.Trim();
        QrToken = qrToken;
        QrTokenHash = qrTokenHash;
        Status = CheckInPassStatus.Active;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid EventId { get; private set; }
    public Guid RegistrationOrderId { get; private set; }
    public Guid RegistrationTicketId { get; private set; }
    public int TicketTypeId { get; private set; }
    public string AttendeeName { get; private set; }
    public string AttendeeEmail { get; private set; }
    public string QrToken { get; private set; }
    public string QrTokenHash { get; private set; }
    public CheckInPassStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CheckedInAt { get; private set; }
    public Guid? CheckedInByUserId { get; private set; }

    public bool CheckIn(Guid staffUserId, DateTimeOffset checkedInAt)
    {
        if (Status == CheckInPassStatus.Cancelled)
            throw new InvalidOperationException("Cancelled passes cannot be checked in.");

        if (Status == CheckInPassStatus.CheckedIn)
            return false;

        Status = CheckInPassStatus.CheckedIn;
        CheckedInAt = checkedInAt;
        CheckedInByUserId = staffUserId == Guid.Empty ? null : staffUserId;
        return true;
    }

    public void Cancel()
    {
        if (Status == CheckInPassStatus.CheckedIn)
            throw new InvalidOperationException("Checked-in passes cannot be cancelled.");

        Status = CheckInPassStatus.Cancelled;
    }
}
