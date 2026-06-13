using Eventbox.EventManagement.EventApi.Domains;

namespace Eventbox.EventManagement.EventApi.Dtos.PublicEvents
{
    public class PublicEventSearchDto : PagingInfo
    {
        public EventStatus? Status { get; set; }
        public string? Q { get; set; }
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Format { get; set; }

    }
}
