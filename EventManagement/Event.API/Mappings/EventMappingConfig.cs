using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.EventManagement.EventApi.Dtos;
using Mapster;

namespace Eventbox.EventManagement.EventApi.Mappings
{
    public class EventMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Eventbox.EventManagement.EventApi.Domains.Event, EventDto>()
                .Map(dest => dest.TicketTypes, src => src.TicketTypes.Adapt<IEnumerable<TicketTypeDto>>());

            config.NewConfig<Eventbox.EventManagement.EventApi.Domains.Event, EventReportDto>()
                .Map(dest => dest.EventId, src => src.Id)
                .Map(dest => dest.TicketTypes, src => src.TicketTypes.Adapt<IEnumerable<TicketTypeReportItemDto>>());

            config.NewConfig<TicketType, TicketTypeDto>();

            config.NewConfig<TicketType, TicketTypeReportItemDto>()
                .Map(dest => dest.TicketTypeId, src => src.Id)
                .Map(dest => dest.TotalQuota, src => src.Quota);
        }
    }
}
