using System.ComponentModel.DataAnnotations;

namespace Eventbox.EventManagement.EventApi.Dtos;

public class UpdateEventRequest
{
    [Required, MinLength(1), MaxLength(200)]
    public string Name { get; set; } = "";

    [Required]
    public DateTimeOffset StartDate { get; set; }

    [Required]
    public DateTimeOffset EndDate { get; set; }

    [MaxLength(500)]
    public string? Summary { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }
}
