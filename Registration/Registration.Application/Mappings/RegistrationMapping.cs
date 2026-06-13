using Eventbox.Registration.Application.Dtos;
using Eventbox.Registration.Domain.Entities.OrderAggregate;
using Eventbox.Registration.Domain.Entities.SeatAvailabilityAggregate;
using Mapster;

namespace Eventbox.Registration.Application.Mappings
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

            config.NewConfig<SeatAvailability, SeatAvailabilityDto>()
                .Map(dest => dest.EventId, src => src.Id)
                .Map(dest => dest.TicketTypes, src => src.TicketTypes.Adapt<IReadOnlyCollection<TicketTypeAvailabilityDto>>());

            config.NewConfig<TicketTypeAvailability, TicketTypeAvailabilityDto>();
        }
    }
}
