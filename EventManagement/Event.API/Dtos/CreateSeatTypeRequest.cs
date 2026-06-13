using System.ComponentModel.DataAnnotations;

namespace Eventbox.EventManagement.EventApi.Dtos;

public class CreateSeatTypeRequest
{
    [Required, MinLength(1), MaxLength(200)]
    public string Name { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Quota must be at least 1.")]
    public int Quota { get; set; }
}
