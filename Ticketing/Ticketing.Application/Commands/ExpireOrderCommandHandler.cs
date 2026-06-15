using Eventbox.TicketingApplication.Abstractions;
using Eventbox.TicketingApplication.Abstractions.Repositories;
using MediatR;

namespace Eventbox.TicketingApplication.Commands
{
    public class ExpireOrderCommandHandler(
        IOrderRepository orderRepository,
        ITicketAvailabilityRepository ticketAvailabilityRepository,
        IRegistrationUnitOfWork unitOfWork) : IRequestHandler<ExpireOrderCommand, bool>
    {
        public async Task<bool> Handle(ExpireOrderCommand request, CancellationToken cancellationToken)
        {
            var order = orderRepository
                .Get(
                    x => x.Id == request.OrderId,
                    includeProperties: "OrderItems",
                    needAsNoTracking: false)
                .FirstOrDefault();
            if (order is null)
                return true;

            var stateChanged = order.Expire(DateTimeOffset.UtcNow);
            if (stateChanged)
            {
                var ticketAvailability = await ticketAvailabilityRepository.GetByEventIdAsync(order.EventId, cancellationToken);
                if (ticketAvailability is not null)
                {
                    foreach (var item in order.OrderItems)
                    {
                        ticketAvailability.Release(item.TicketTypeId, item.Quantity);
                    }

                    ticketAvailabilityRepository.Update(ticketAvailability);
                }

                orderRepository.Update(order);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
    }
}
