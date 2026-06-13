using Eventbox.EventManagement.EventApi.Domains;

namespace Eventbox.EventManagement.EventApi.Dtos.OrganizerEvents
{
    public class OrganizerEventDto
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = default!;
        public string? Description { get; private set; }
        public string Slug { get; private set; } = default!;
        public DateOnly StartDate { get; private set; }
        public DateOnly EndDate { get; private set; }
        public bool IsPublished { get; private set; }
        public string? AccessCode { get; private set; }
        public int SeatCount { get; private set; }
        public IEnumerable<SeatTypeDto> Seats { get; private set; }
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
    }
}
