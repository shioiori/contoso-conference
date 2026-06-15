using Eventbox.EventManagement.EventApi.Domains;

namespace Eventbox.EventManagement.EventApi.Dtos.PublicEvents
{
    public class PublicEventDto
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = default!;
        public string? Description { get; private set; }
        public string Slug { get; private set; } = default!;
        public DateTimeOffset From { get; private set; }
        public DateTimeOffset To { get; private set; }
        public bool IsPublished { get; private set; }
        public string? AccessCode { get; private set; }
        public int TicketCount { get; private set; }
        public IEnumerable<PublicTicketTypeDto> TicketTypes { get; set; } = [];
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
    }
}
