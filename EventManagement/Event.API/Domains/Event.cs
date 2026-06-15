using Eventbox.EventManagement.EventApi.Domains.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eventbox.EventManagement.EventApi.Domains
{
    [Table("Event")]
    public class Event : Entity<Guid>
    {
        public Guid OrganizationId { get; private set; }
        public string Name { get; private set; } = default!;
        public string? Description { get; private set; }
        public string Slug { get; private set; } = default!;
        public DateTimeOffset From { get; private set; }
        public DateTimeOffset To { get; private set; }
        public bool IsPublished { get; private set; }
        public string? AccessCode { get; private set; }

        public EventStatus Status
        {
            get
            {
                if (IsPublished)
                    return EventStatus.Published;
                else if (From > DateTimeOffset.UtcNow)
                    return EventStatus.Draft;
                else
                    return EventStatus.Cancelled;
            }
        }

        private readonly List<TicketType> _ticketTypes = [];
        public IReadOnlyCollection<TicketType> TicketTypes => _ticketTypes.AsReadOnly();

        private Event() { }

        public Event(Guid id, Guid organizationId, string name, string slug, DateTimeOffset from, DateTimeOffset to, string? description, string accessCode)
        {
            from = from.ToUniversalTime();
            to = to.ToUniversalTime();

            if (to <= from)
                throw new ArgumentException("To must be after From.", nameof(to));

            Id = id;
            OrganizationId = organizationId;
            Name = name;
            Slug = slug;
            From = from;
            To = to;
            Description = description;
            AccessCode = accessCode;
        }

        public void Update(string name, DateTimeOffset from, DateTimeOffset to, string? description)
        {
            from = from.ToUniversalTime();
            to = to.ToUniversalTime();

            if (to <= from)
                throw new ArgumentException("To must be after From.", nameof(to));

            Name = name;
            From = from;
            To = to;
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
