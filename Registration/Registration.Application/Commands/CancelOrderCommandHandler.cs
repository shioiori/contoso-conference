using MediatR;
using Eventbox.Registration.Application.Abstractions;

namespace Eventbox.Registration.Application.Commands
{
    public class CancelOrderCommandHandler(
        IUnitOfWork unitOfWork) : IRequestHandler<CancelOrderCommand, bool>
    {
        public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            var order = unitOfWork.Orders
                .Get(
                    x => x.Id == request.OrderId,
                    includeProperties: "OrderItems",
                    needAsNoTracking: false)
                .FirstOrDefault()
                ?? throw new KeyNotFoundException($"Order '{request.OrderId}' was not found.");

            var stateChanged = order.Cancel();

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
