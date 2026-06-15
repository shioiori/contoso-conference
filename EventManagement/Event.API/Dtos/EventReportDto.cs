namespace Eventbox.EventManagement.EventApi.Dtos
{
    public class EventReportDto
    {
        public Guid EventId { get; set; }
        public string Name { get; set; } = "";
        public IEnumerable<TicketTypeReportItemDto> TicketTypes { get; set; } = [];
    }

    public class TicketTypeReportItemDto
    {
        public int TicketTypeId { get; set; }
        public string Name { get; set; } = "";
        public int TotalQuota { get; set; }
    }
}
