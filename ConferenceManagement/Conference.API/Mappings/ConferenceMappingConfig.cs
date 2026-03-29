using Conference.API.Domains;
using Conference.API.Dtos;
using Mapster;

namespace Conference.API.Mappings
{
    public class ConferenceMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Conference.API.Domains.Conference, ConferenceResponse>()
                .Map(dest => dest.Seats, src => src.Seats.Adapt<IEnumerable<SeatTypeResponse>>());

            config.NewConfig<Conference.API.Domains.Conference, ConferenceReportResponse>()
                .Map(dest => dest.ConferenceId, src => src.Id)
                .Map(dest => dest.SeatTypes, src => src.Seats.Adapt<IEnumerable<SeatTypeReportItem>>());

            config.NewConfig<SeatType, SeatTypeResponse>();

            config.NewConfig<SeatType, SeatTypeReportItem>()
                .Map(dest => dest.SeatTypeId, src => src.Id)
                .Map(dest => dest.TotalQuota, src => src.Quota);
        }
    }
}
