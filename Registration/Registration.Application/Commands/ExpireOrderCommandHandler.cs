using Eventbox.Registration.Application.Abstractions;
using MediatR;

namespace Eventbox.Registration.Application.Commands
{
    public class ExpireOrderCommandHandler(
        IUnitOfWork unitOfWork) : IRequestHandler<ExpireOrderCommand, bool>
    {
        public async Task<bool> Handle(ExpireOrderCommand request, CancellationToken cancellationToken)
        {
            var order = unitOfWork.Orders
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
                var seatAvailability = await unitOfWork.SeatAvailabilities.GetByEventIdAsync(order.EventId, cancellationToken);
                if (seatAvailability is not null)
                {
                    foreach (var item in order.OrderItems)
                    {
                        seatAvailability.Release(item.TicketTypeId, item.Quantity);
                    }

                    unitOfWork.SeatAvailabilities.Update(seatAvailability);
                }

                unitOfWork.Orders.Update(order);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
    }
}
