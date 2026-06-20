using System.ComponentModel.DataAnnotations;
using Eventbox.EventManagement.EventApi.Dtos;
using Eventbox.EventManagement.EventApi.Enums;

namespace Eventbox.EventManagement.EventApi.Requests;

public class UpdateTicketTypeRequest
{
    [Required, MinLength(1), MaxLength(200)]
    public string Name { get; set; } = "";

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Quota cannot be negative.")]
    public int Quota { get; set; }

    [Required, MinLength(3), MaxLength(3)]
    public string Currency { get; set; } = "VND";

    [Range(1, int.MaxValue, ErrorMessage = "Minimum per order must be at least 1.")]
    public int MinPerOrder { get; set; } = 1;

    [Range(1, int.MaxValue, ErrorMessage = "Maximum per order must be at least 1.")]
    public int? MaxPerOrder { get; set; }

    public TicketVisibility Visibility { get; set; } = TicketVisibility.Public;

    public string? AccessCode { get; set; }

    [MinLength(1, ErrorMessage = "At least one pricing phase is required.")]
    public List<PricingPhaseDto> PricingPhases { get; set; } = [];
}
