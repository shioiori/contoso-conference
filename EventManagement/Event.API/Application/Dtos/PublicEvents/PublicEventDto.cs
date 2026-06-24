using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.EventManagement.EventApi.Domains.Enums;
using Eventbox.EventManagement.EventApi.Domains.Extensions;

namespace Eventbox.EventManagement.EventApi.Application.Dtos.PublicEvents
{
    public class PublicEventDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string Slug { get; set; } = default!;
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
        public bool IsPublished { get; set; }
        public string? AccessCode { get; set; }
        public int TicketCount { get; set; }
        public IEnumerable<PublicTicketTypeDto> TicketTypes { get; set; } = [];
        public EventStatus Status => EventStatusHelper.GetCurrentEventStatus(IsPublished, From);
    }
}
