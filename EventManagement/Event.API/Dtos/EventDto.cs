namespace Eventbox.EventManagement.EventApi.Dtos
{
    public class EventDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string Slug { get; set; } = "";
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
        public bool IsPublished { get; set; }
        public IEnumerable<TicketTypeDto> TicketTypes { get; set; } = [];
    }
}
