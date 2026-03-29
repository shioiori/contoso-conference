using System.ComponentModel.DataAnnotations;

namespace Conference.API.Dtos;

public record UpdateConferenceRequest(
    [Required, MinLength(1), MaxLength(200)] string Name,
    [Required] DateTime StartDate,
    [Required] DateTime EndDate,
    [MaxLength(2000)] string? Description
);
