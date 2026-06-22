using Eventbox.EventManagement.EventApi.Domains.Enums;

namespace Eventbox.EventManagement.EventApi.Application.Dtos
{
    public class TicketTypeInputDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quota { get; set; }
        public string Currency { get; set; }
        public int MinPerOrder { get; set; }
        public int? MaxPerOrder { get; set; }
        public TicketVisibility Visibility { get; set; }
        public string AccessCode { get; set; }
        public List<PricingPhaseDto> PricingPhases { get; set; }
    }
}
