using System.ComponentModel.DataAnnotations;

namespace Eventbox.EventManagement.EventApi.Api.Requests;

public class OrganizationRequest
{
    [Required, MinLength(1), MaxLength(200)]
    public string Name { get; set; } = "";
}
