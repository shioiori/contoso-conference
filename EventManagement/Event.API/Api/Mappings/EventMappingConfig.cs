using Eventbox.EventManagement.EventApi.Application.Dtos;
using Eventbox.EventManagement.EventApi.Domains;
using Mapster;
using Event = Eventbox.EventManagement.EventApi.Domains.Event;
namespace Eventbox.EventManagement.EventApi.Api.Mappings
{
    public class EventMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Event, EventDto>()
                .Map(dest => dest.TicketTypes, src => src.TicketTypes.Adapt<IEnumerable<TicketTypeDto>>());

            config.NewConfig<TicketType, TicketTypeDto>();
        }
    }
}
