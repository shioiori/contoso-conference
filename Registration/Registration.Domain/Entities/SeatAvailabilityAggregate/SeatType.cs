using Registration.Domain.SeedWork;

namespace Registration.Domain.Entities.SeatAvailabilityAggregate
{
    public class SeatType
    {
        public int SeatTypeId { get; set; }
        public int Quantity { get; set; }
        public int Remaining { get; set; }
    }
}
