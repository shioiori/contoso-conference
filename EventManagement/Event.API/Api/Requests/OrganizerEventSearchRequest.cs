using Eventbox.EventManagement.EventApi.Application.Dtos;
using Eventbox.EventManagement.EventApi.Domains.Enums;

namespace Eventbox.EventManagement.EventApi.Api.Requests;

public class OrganizerEventSearchRequest : PagingInfo
{
    public Guid? OrganizationId { get; set; }
    public EventStatus? Status { get; set; }
    public string? Q { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
}
