using Eventbox.Ticketing.Application.Abstractions;
using MediatR;

namespace Eventbox.Ticketing.Application.Commands
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
                var ticketAvailability = await unitOfWork.TicketAvailabilities.GetByEventIdAsync(order.EventId, cancellationToken);
                if (ticketAvailability is not null)
                {
                    foreach (var item in order.OrderItems)
                    {
                        ticketAvailability.Release(item.TicketTypeId, item.Quantity);
                    }

                    unitOfWork.TicketAvailabilities.Update(ticketAvailability);
                }

                unitOfWork.Orders.Update(order);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
    }
}
