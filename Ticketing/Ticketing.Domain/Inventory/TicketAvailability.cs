using Eventbox.Shared.SeedWorks;

namespace Eventbox.Ticketing.Domain.Inventory
{
    public class TicketAvailability : AuditableEntity<Guid>
    {
        private readonly List<TicketTypeAvailability> _ticketTypes = new();

        public TicketAvailability(Guid eventId, IEnumerable<TicketTypeAvailability> ticketTypes) : base(eventId)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("Event id is required.", nameof(eventId));
            _ticketTypes.AddRange(ticketTypes);
        }

        public IReadOnlyCollection<TicketTypeAvailability> TicketTypes => _ticketTypes.AsReadOnly();

        public TicketTypeAvailability GetTicketType(Guid ticketTypeId)
            => _ticketTypes.FirstOrDefault(s => s.Id == ticketTypeId)
                ?? throw new KeyNotFoundException($"Ticket type '{ticketTypeId}' was not found.");

        public void Reserve(Guid ticketTypeId, int quantity)
        {
            GetTicketType(ticketTypeId).Reserve(quantity);
        }

        public void Release(Guid ticketTypeId, int quantity)
        {
            GetTicketType(ticketTypeId).Release(quantity);
        }

        public void AddTicketType(TicketTypeAvailability ticketType)
        {
            if (_ticketTypes.Any(existing => existing.Id == ticketType.Id))
                throw new InvalidOperationException($"Ticket type '{ticketType.Id}' already exists.");

            _ticketTypes.Add(ticketType);
        }

        public void IncreaseTicketTypeQuantity(Guid ticketTypeId, int quantity)
        {
            GetTicketType(ticketTypeId).IncreaseQuantity(quantity);
        }

        public void RemoveTicketType(Guid ticketTypeId)
        {
            var ticketType = _ticketTypes.FirstOrDefault(s => s.Id == ticketTypeId);
            if (ticketType is not null)
                _ticketTypes.Remove(ticketType);
        }
    }
}
