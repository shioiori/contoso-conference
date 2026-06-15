using Eventbox.TicketingApplication.Dtos;
using Eventbox.TicketingDomain.Entities.OrderAggregate;
using Eventbox.TicketingDomain.Entities.TicketAvailabilityAggregate;
using Mapster;

namespace Eventbox.TicketingApplication.Mappings
{
    public class RegistrationMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Order, OrderDto>()
                .Map(dest => dest.OrderState, src => src.GetCurrentState(DateTimeOffset.UtcNow))
                .Map(dest => dest.Name, src => src.PersonalInfo.Name)
                .Map(dest => dest.Email, src => src.PersonalInfo.Email)
                .Map(dest => dest.Items, src => src.OrderItems.Adapt<IReadOnlyCollection<OrderItemDto>>());

            config.NewConfig<OrderItem, OrderItemDto>();

            config.NewConfig<TicketAvailability, TicketAvailabilityDto>()
                .Map(dest => dest.EventId, src => src.Id)
                .Map(dest => dest.TicketTypes, src => src.TicketTypes.Adapt<IReadOnlyCollection<TicketTypeAvailabilityDto>>());

            config.NewConfig<TicketTypeAvailability, TicketTypeAvailabilityDto>()
                .Map(dest => dest.Visibility, src => src.Visibility.ToString())
                .Map(dest => dest.PricingPhases, src => src.PricingPhases.Adapt<IReadOnlyCollection<PricingPhaseAvailabilityDto>>());

            config.NewConfig<PricingPhaseAvailability, PricingPhaseAvailabilityDto>();
        }
    }
}
