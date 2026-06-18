using Eventbox.Ticketing.Domain.SeedWork;

namespace Eventbox.Ticketing.Domain.Entities.TicketAvailabilityAggregate
{
    public class TicketAvailability : Entity<Guid>
    {
        private readonly List<TicketTypeAvailability> _ticketTypes = new();

        private TicketAvailability()
        {
        }

        public TicketAvailability(Guid eventId, IEnumerable<TicketTypeAvailability> ticketTypes)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("Event id is required.", nameof(eventId));

            Id = eventId;
            _ticketTypes.AddRange(ticketTypes);
        }

        public IReadOnlyCollection<TicketTypeAvailability> TicketTypes => _ticketTypes.AsReadOnly();

        public TicketTypeAvailability GetTicketType(int ticketTypeId)
            => _ticketTypes.FirstOrDefault(s => s.Id == ticketTypeId)
                ?? throw new KeyNotFoundException($"Ticket type '{ticketTypeId}' was not found.");

        public void Reserve(int ticketTypeId, int quantity)
        {
            GetTicketType(ticketTypeId).Reserve(quantity);
        }

        public void Release(int ticketTypeId, int quantity)
        {
            GetTicketType(ticketTypeId).Release(quantity);
        }

        public void AddTicketType(TicketTypeAvailability ticketType)
        {
            if (_ticketTypes.Any(existing => existing.Id == ticketType.Id))
                throw new InvalidOperationException($"Ticket type '{ticketType.Id}' already exists.");

            _ticketTypes.Add(ticketType);
        }

        public void IncreaseTicketTypeQuantity(int ticketTypeId, int quantity)
        {
            GetTicketType(ticketTypeId).IncreaseQuantity(quantity);
        }

        public void RemoveTicketType(int ticketTypeId)
        {
            var ticketType = _ticketTypes.FirstOrDefault(s => s.Id == ticketTypeId);
            if (ticketType is not null)
                _ticketTypes.Remove(ticketType);
        }
    }
}
