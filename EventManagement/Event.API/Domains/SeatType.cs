using Eventbox.EventManagement.EventApi.Domains.Common;

namespace Eventbox.EventManagement.EventApi.Domains
{
    public class SeatType : Entity<int>
    {
        public string Name { get; private set; } = default!;
        public Guid EventId { get; private set; }
        public int Quota { get; private set; }
        public Event Event { get; private set; } = default!;

        public SeatType(string name, Guid eventId, int quota)
        {
            Name = name;
            EventId = eventId;
            Quota = quota;
        }

        public void IncreaseQuota(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity to add must be positive.");
            Quota += quantity;
        }
    }
}
