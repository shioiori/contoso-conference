using MediatR;
using Eventbox.Registration.Application.Abstractions;
using Eventbox.Registration.Application.Abstractions.Repositories;

namespace Eventbox.Registration.Application.Commands
{
    public class CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        ISeatAvailabilityRepository seatAvailabilityRepository,
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
                ?? throw new KeyNotFoundException($"Order '{request.OrderId}' was not found.");

            var stateChanged = order.Cancel();

            if (stateChanged)
            {
                var seatAvailability = await seatAvailabilityRepository.GetByEventIdAsync(order.EventId, cancellationToken);
                if (seatAvailability is not null)
                {
                    foreach (var item in order.OrderItems)
                    {
                        seatAvailability.Release(item.TicketTypeId, item.Quantity);
                    }

                    seatAvailabilityRepository.Update(seatAvailability);
                }

                orderRepository.Update(order);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
    }
}
