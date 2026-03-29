using Conference.API.Domains.Common;

namespace Conference.API.Domains
{
    public class SeatType : Entity<int>
    {
        public string Name { get; private set; } = default!;
        public Guid ConferenceId { get; private set; }
        public int Quota { get; private set; }

        public Conference Conference { get; private set; } = default!;

        private SeatType() { }

        public SeatType(string name, Guid conferenceId, int quota)
        {
            Name = name;
            ConferenceId = conferenceId;
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
