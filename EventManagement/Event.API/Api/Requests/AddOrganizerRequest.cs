using System.ComponentModel.DataAnnotations;

namespace Eventbox.EventManagement.EventApi.Api.Requests;

public record AddOrganizerRequest
{
    [Required]
    public Guid OrganizerId { get; init; }
}
