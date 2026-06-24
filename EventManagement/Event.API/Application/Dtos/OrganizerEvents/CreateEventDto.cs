namespace Eventbox.EventManagement.EventApi.Application.Dtos.OrganizerEvents
{
    public class CreateEventDto
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
        public string? Description { get; set; }
    }
}
