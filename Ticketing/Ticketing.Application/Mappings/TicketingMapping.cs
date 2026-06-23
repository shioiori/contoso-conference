using Eventbox.Ticketing.Domain.Orders;
using Eventbox.Ticketing.Domain.Inventory;
using Eventbox.Ticketing.Domain.Tickets;
using Mapster;
using Eventbox.Ticketing.Application.Dtos.Inventory;
using Eventbox.Ticketing.Application.Dtos.Orders;
using Eventbox.Ticketing.Application.Dtos.Tickets;
using Eventbox.Ticketing.Domain.Enums;

namespace Eventbox.Ticketing.Application.Mappings
{
    public class TicketingMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Order, OrderDto>()
                .Map(dest => dest.OrderState, src => src.GetCurrentState(DateTimeOffset.UtcNow))
                .Map(dest => dest.Name, src => src.PersonalInfo.Name)
                .Map(dest => dest.Email, src => src.PersonalInfo.Email)
                .Map(dest => dest.Items, src => src.OrderItems.Adapt<IReadOnlyCollection<OrderItemDto>>())
                .Ignore(dest => dest.Tickets);

            config.NewConfig<OrderItem, OrderItemDto>();

            config.NewConfig<Ticket, TicketDto>()
                .Map(dest => dest.CheckInStatus, src => src.TicketState == TicketState.Cancelled
                    ? CheckInStatus.Cancelled
                    : src.CheckedInAt.HasValue
                        ? CheckInStatus.CheckedIn
                        : CheckInStatus.Active);

            config.NewConfig<TicketTypeAvailability, TicketTypeAvailabilityDto>()
                .Map(dest => dest.Visibility, src => src.Visibility.ToString())
                .Map(dest => dest.PricingPhases, src => src.PricingPhases.Adapt<IReadOnlyCollection<PricingPhaseAvailabilityDto>>());

            config.NewConfig<PricingPhaseAvailability, PricingPhaseAvailabilityDto>();
        }
    }
}
