namespace Eventbox.EventManagement.EventApi.Application.Dtos.OrganizerEvents
{
    public class UpdateEventDto
    {
        public string Name { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
        public string? Description { get; set; }
    }
}
