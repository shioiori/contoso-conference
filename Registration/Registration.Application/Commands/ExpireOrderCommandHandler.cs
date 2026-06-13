using Eventbox.Registration.Application.Abstractions;
using Eventbox.Registration.Application.Abstractions.Repositories;
using MediatR;

namespace Eventbox.Registration.Application.Commands
{
    public class ExpireOrderCommandHandler(
        IOrderRepository orderRepository,
        ISeatAvailabilityRepository seatAvailabilityRepository,
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
