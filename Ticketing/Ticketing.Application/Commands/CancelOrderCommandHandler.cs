using MediatR;
using Eventbox.TicketingApplication.Abstractions;
using Eventbox.TicketingApplication.Abstractions.Repositories;
using Eventbox.Shared.Exceptions;

namespace Eventbox.TicketingApplication.Commands
{
    public class CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        ITicketAvailabilityRepository ticketAvailabilityRepository,
        IRegistrationUnitOfWork unitOfWork) : IRequestHandler<CancelOrderCommand, bool>
    {
        public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            var order = orderRepository
                .Get(
                    x => x.Id == request.OrderId,
                    includeProperties: "OrderItems",
                    needAsNoTracking: false)
                .FirstOrDefault()
                ?? throw new NotFoundException("Order", request.OrderId);

            var stateChanged = order.Cancel();

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
