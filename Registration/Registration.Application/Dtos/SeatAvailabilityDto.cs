namespace Eventbox.Registration.Application.Dtos
{
    public class SeatAvailabilityDto
    {
        public SeatAvailabilityDto(Guid eventId, IReadOnlyCollection<TicketTypeAvailabilityDto> ticketTypes)
        {
            EventId = eventId;
            TicketTypes = ticketTypes;
        }

        public Guid EventId { get; }
        public IReadOnlyCollection<TicketTypeAvailabilityDto> TicketTypes { get; }
    }

    public class TicketTypeAvailabilityDto
    {
        public TicketTypeAvailabilityDto(int ticketTypeId, int quantity, int remaining)
        {
            TicketTypeId = ticketTypeId;
            Quantity = quantity;
            Remaining = remaining;
        }

        public int TicketTypeId { get; }
        public int Quantity { get; }
        public int Remaining { get; }
    }
}
