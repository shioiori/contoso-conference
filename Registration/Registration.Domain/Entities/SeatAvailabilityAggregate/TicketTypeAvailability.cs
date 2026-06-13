using Eventbox.Registration.Domain.SeedWork;

namespace Eventbox.Registration.Domain.Entities.SeatAvailabilityAggregate
{
    public class TicketTypeAvailability
    {
        public TicketTypeAvailability(int ticketTypeId, int quantity)
        {
            if (ticketTypeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(ticketTypeId), "Ticket type id must be greater than zero.");

            if (quantity < 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative.");

            TicketTypeId = ticketTypeId;
            Quantity = quantity;
            Remaining = quantity;
        }

        public int TicketTypeId { get; private set; }
        public int Quantity { get; private set; }
        public int Remaining { get; private set; }

        public void Reserve(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

            if (Remaining < quantity)
                throw new InvalidOperationException("Not enough seats remaining.");

            Remaining -= quantity;
        }

        public void Release(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

            Remaining = Math.Min(Quantity, Remaining + quantity);
        }
    }
}
