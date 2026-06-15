using Eventbox.TicketingDomain.Enums;
using Eventbox.TicketingDomain.SeedWork;

namespace Eventbox.TicketingDomain.Entities.OrderAggregate;

public class Ticket : Entity<Guid>
{
    private Ticket()
    {
    }

    public Ticket(int ticketTypeId, int sequenceNumber)
    {
        if (ticketTypeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(ticketTypeId), "Ticket type id must be greater than zero.");

        if (sequenceNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(sequenceNumber), "Sequence number must be greater than zero.");

        Id = Guid.NewGuid();
        TicketTypeId = ticketTypeId;
        SequenceNumber = sequenceNumber;
        TicketState = TicketState.Active;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public int TicketTypeId { get; private set; }
    public int SequenceNumber { get; private set; }
    public TicketState TicketState { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public void Cancel()
    {
        TicketState = TicketState.Cancelled;
    }
}
