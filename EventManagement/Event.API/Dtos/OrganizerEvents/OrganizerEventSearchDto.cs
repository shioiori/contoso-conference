using Eventbox.EventManagement.EventApi.Domains;

namespace Eventbox.EventManagement.EventApi.Dtos.OrganizerEvents
{
    public class OrganizerEventSearchDto : PagingInfo
    {
        public Guid? OrganizationId { get; set; }
        public EventStatus? Status { get; set; }
        public string? Q { get; set; }
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
    }
}
