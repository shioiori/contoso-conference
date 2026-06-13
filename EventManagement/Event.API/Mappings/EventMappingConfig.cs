using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.EventManagement.EventApi.Dtos;
using Mapster;

namespace Eventbox.EventManagement.EventApi.Mappings
{
    public class EventMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Eventbox.EventManagement.EventApi.Domains.Event, EventResponse>()
                .Map(dest => dest.Seats, src => src.Seats.Adapt<IEnumerable<SeatTypeResponse>>());

            config.NewConfig<Eventbox.EventManagement.EventApi.Domains.Event, EventReportResponse>()
                .Map(dest => dest.EventId, src => src.Id)
                .Map(dest => dest.SeatTypes, src => src.Seats.Adapt<IEnumerable<SeatTypeReportItem>>());

            config.NewConfig<SeatType, SeatTypeResponse>();

            config.NewConfig<SeatType, SeatTypeReportItem>()
                .Map(dest => dest.SeatTypeId, src => src.Id)
                .Map(dest => dest.TotalQuota, src => src.Quota);
        }
    }
}
