namespace Eventbox.EventManagement.EventApi.Dtos
{
    public class EventReportResponse
    {
        public Guid EventId { get; set; }
        public string Name { get; set; } = "";
        public IEnumerable<SeatTypeReportItem> SeatTypes { get; set; } = [];
    }

    public class SeatTypeReportItem
    {
        public int SeatTypeId { get; set; }
        public string Name { get; set; } = "";
        public int TotalQuota { get; set; }
    }
}
