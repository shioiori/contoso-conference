using Eventbox.EventManagement.EventApi.Application.Dtos;
using Eventbox.EventManagement.EventApi.Domains.Enums;

namespace Eventbox.EventManagement.EventApi.Api.Requests;

public class PublicEventSearchRequest : PagingInfo
{
    public EventStatus? Status { get; set; }
    public string? Q { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
