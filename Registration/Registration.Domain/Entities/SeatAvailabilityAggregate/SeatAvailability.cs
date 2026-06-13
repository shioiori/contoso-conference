using Eventbox.Registration.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eventbox.Registration.Domain.Entities.SeatAvailabilityAggregate
{
    public class SeatAvailability : Entity<Guid>
    {
        private readonly List<TicketTypeAvailability> _ticketTypes = new();

        public SeatAvailability(Guid eventId, IEnumerable<TicketTypeAvailability> ticketTypes)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("Event id is required.", nameof(eventId));

            Id = eventId;
            _ticketTypes.AddRange(ticketTypes);
        }

        public IReadOnlyCollection<TicketTypeAvailability> TicketTypes => _ticketTypes.AsReadOnly();

        public void Reserve(int ticketTypeId, int quantity)
        {
            var ticketType = _ticketTypes.FirstOrDefault(s => s.TicketTypeId == ticketTypeId)
                ?? throw new KeyNotFoundException($"Ticket type '{ticketTypeId}' was not found.");

            ticketType.Reserve(quantity);
        }

        public void Release(int ticketTypeId, int quantity)
        {
            var ticketType = _ticketTypes.FirstOrDefault(s => s.TicketTypeId == ticketTypeId)
                ?? throw new KeyNotFoundException($"Ticket type '{ticketTypeId}' was not found.");

            ticketType.Release(quantity);
        }
    }
}
