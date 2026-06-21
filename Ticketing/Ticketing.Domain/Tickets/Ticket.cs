using Eventbox.Ticketing.Domain.Enums;
using Eventbox.Ticketing.Domain.SeedWork;

namespace Eventbox.Ticketing.Domain.Tickets;

public class Ticket : Entity<Guid>
{
    private Ticket()
    {
        QrToken = null!;
        QrTokenHash = null!;
    }

    public Ticket(Guid orderId, Guid eventId, Guid ticketTypeId, int sequenceNumber)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("Order id is required.", nameof(orderId));

        if (eventId == Guid.Empty)
            throw new ArgumentException("Event id is required.", nameof(eventId));

        if (ticketTypeId == Guid.Empty)
            throw new ArgumentException("Ticket type id must not be empty.", nameof(ticketTypeId));

        if (sequenceNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(sequenceNumber), "Sequence number must be greater than zero.");

        Id = Guid.NewGuid();
        OrderId = orderId;
        EventId = eventId;
        TicketTypeId = ticketTypeId;
        SequenceNumber = sequenceNumber;
        TicketState = TicketState.Active;
    }

    public Guid OrderId { get; private set; }
    public Guid EventId { get; private set; }
    public Guid TicketTypeId { get; private set; }
    public int SequenceNumber { get; private set; }
    public TicketState TicketState { get; private set; }
    public string? QrToken { get; private set; }
    public string? QrTokenHash { get; private set; }
    public DateTimeOffset? CheckedInAt { get; private set; }
    public Guid? CheckedInByUserId { get; private set; }

    public void AssignQrToken(string qrToken, string qrTokenHash)
    {
        if (string.IsNullOrWhiteSpace(qrToken))
            throw new ArgumentException("QR token is required.", nameof(qrToken));

        if (string.IsNullOrWhiteSpace(qrTokenHash))
            throw new ArgumentException("QR token hash is required.", nameof(qrTokenHash));

        QrToken = qrToken;
        QrTokenHash = qrTokenHash;
    }

    public bool CheckIn(Guid staffUserId, DateTimeOffset checkedInAt)
    {
        if (TicketState == TicketState.Cancelled)
            throw new InvalidOperationException("Cancelled tickets cannot be checked in.");

        if (CheckedInAt.HasValue)
            return false;

        CheckedInAt = checkedInAt;
        CheckedInByUserId = staffUserId == Guid.Empty ? null : staffUserId;
        return true;
    }

    public void Cancel()
    {
        if (CheckedInAt.HasValue)
            throw new InvalidOperationException("Checked-in tickets cannot be cancelled.");

        TicketState = TicketState.Cancelled;
    }
}
