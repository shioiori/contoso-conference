using System.ComponentModel.DataAnnotations;

namespace Conference.API.Dtos
{
    public record CreateConferenceRequest(
        [Required, MinLength(1), MaxLength(200)] string Name,
        [Required, MinLength(1), MaxLength(100)] string Slug,
        [Required] DateTime StartDate,
        [Required] DateTime EndDate,
        [MaxLength(2000)] string? Description
    );
}
