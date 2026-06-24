using Eventbox.EventManagement.EventApi.Domains.Enums;
using Eventbox.EventManagement.EventApi.Domains.Extensions;
using Eventbox.Shared.Exceptions;
using Eventbox.Shared.SeedWorks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eventbox.EventManagement.EventApi.Domains
{
    [Table("Event")]
    public class Event : AuditableEntity<Guid>
    {
        public Guid OrganizationId { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string Slug { get; set; } = default!;
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
        public bool IsPublished { get; set; }
        public string? AccessCode { get; set; }

        public EventStatus Status => EventStatusHelper.GetCurrentEventStatus(IsPublished, From);

        private readonly List<TicketType> _ticketTypes = [];
        public IReadOnlyCollection<TicketType> TicketTypes => _ticketTypes.AsReadOnly();

        private Event() { }

        public Event(Guid id, Guid organizationId, string name, string slug, DateTimeOffset from, DateTimeOffset to, string? description, string accessCode)
            : base(id)
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

            var utcNow = DateTimeOffset.UtcNow;

            if (utcNow < From && from < utcNow && IsPublished)
                throw new ValidationApiException("Start date cannot be set to a past time.");

            if (utcNow >= From)
            {
                if (utcNow >= To)
                    throw new ValidationApiException("Cannot edit an event that has already ended.");

                if (name != Name || from != From || description != Description)
                    throw new ValidationApiException("Only the end date can be edited while the event is active.");

                if (to <= utcNow)
                    throw new ValidationApiException("End date cannot be set to a past time.");
            }

            Name = name;
            From = from;
            To = to;
            Description = description;
        }

        public bool IsReadyToPublish =>
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrWhiteSpace(Slug) &&
            To > From &&
            _ticketTypes.Count > 0;

        public void Publish()
        {
            if (IsPublished) return;

            if (!IsReadyToPublish)
                throw new ValidationApiException("Event is not ready to publish.");

            IsPublished = true;
        }

        public void Unpublish()
        {
            if (!IsPublished) return;

            var utcNow = DateTimeOffset.UtcNow;
            if (From <= utcNow && utcNow <= To)
                throw new ValidationApiException("Cannot unpublish an event while it is in progress.");

            IsPublished = false;
        }
    }
}