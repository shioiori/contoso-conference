using System.ComponentModel.DataAnnotations;

namespace Conference.API.Dtos;

public record CreateSeatTypeRequest(
    [Required] Guid ConferenceId,
    [Required, MinLength(1), MaxLength(200)] string Name,
    [Range(1, int.MaxValue, ErrorMessage = "Quota must be at least 1.")] int Quota
);
