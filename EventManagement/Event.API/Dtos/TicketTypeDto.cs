using Eventbox.EventManagement.EventApi.Enums;

namespace Eventbox.EventManagement.EventApi.Dtos
{
    public class TicketTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public Guid EventId { get; set; }
        public int Quota { get; set; }
        public string Currency { get; set; } = "VND";
        public int MinPerOrder { get; set; }
        public int? MaxPerOrder { get; set; }
        public TicketVisibility Visibility { get; set; }
        public string? AccessCodeHash { get; set; }
        public IEnumerable<PricingPhaseDto> PricingPhases { get; set; } = [];
    }
}
