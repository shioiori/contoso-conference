using Conference.API.Domains.Common;

namespace Conference.API.Domains
{
    public class Conference : Entity<Guid>
    {
        public string Name { get; private set; } = default!;
        public string? Description { get; private set; }
        public string Slug { get; private set; } = default!;
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public bool IsPublished { get; private set; }
        public string? AccessCode { get; private set; }

        private readonly List<SeatType> _seats = [];
        public IReadOnlyCollection<SeatType> Seats => _seats.AsReadOnly();

        private Conference() { }

        public Conference(Guid id, string name, string slug, DateTime startDate, DateTime endDate, string? description, string accessCode)
        {
            if (endDate <= startDate)
                throw new ArgumentException("EndDate must be after StartDate.", nameof(endDate));

            Id = id;
            Name = name;
            Slug = slug;
            StartDate = startDate;
            EndDate = endDate;
            Description = description;
            AccessCode = accessCode;
        }

        public void Update(string name, DateTime startDate, DateTime endDate, string? description)
        {
            if (endDate <= startDate)
                throw new ArgumentException("EndDate must be after StartDate.", nameof(endDate));

            Name = name;
            StartDate = startDate;
            EndDate = endDate;
            Description = description;
        }

        public void Publish() => IsPublished = true;
        public void Unpublish() => IsPublished = false;
    }
}
