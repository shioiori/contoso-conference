using Eventbox.EventManagement.EventApi.Domains.Common;

namespace Eventbox.EventManagement.EventApi.Domains
{
    public class Event : Entity<Guid>
    {
        public Guid OrganizationId { get; private set; }
        public string Name { get; private set; } = default!;
        public string? Description { get; private set; }
        public string Slug { get; private set; } = default!;
        public DateOnly StartDate { get; private set; }
        public DateOnly EndDate { get; private set; }
        public bool IsPublished { get; private set; }
        public string? AccessCode { get; private set; }

        public EventStatus Status
        {
            get
            {
                if (IsPublished)
                    return EventStatus.Published;
                else if (StartDate > DateOnly.FromDateTime(DateTime.UtcNow))
                    return EventStatus.Draft;
                else
                    return EventStatus.Cancelled;
            }
        }

        private readonly List<SeatType> _seats = [];
        public IReadOnlyCollection<SeatType> Seats => _seats.AsReadOnly();

        private Event() { }

        public Event(Guid id, Guid organizationId, string name, string slug, DateOnly startDate, DateOnly endDate, string? description, string accessCode)
        {
            if (endDate <= startDate)
                throw new ArgumentException("EndDate must be after StartDate.", nameof(endDate));

            Id = id;
            OrganizationId = organizationId;
            Name = name;
            Slug = slug;
            StartDate = startDate;
            EndDate = endDate;
            Description = description;
            AccessCode = accessCode;
        }

        public void Update(string name, DateOnly startDate, DateOnly endDate, string? description)
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

    public enum EventStatus
    {
        Draft,
        Published,
        Cancelled
    }
}
