using Eventbox.Ticketing.Application.Abstractions;
using MediatR;

namespace Eventbox.Ticketing.Application.Commands.Orders
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
                var ticketTypes = await unitOfWork.TicketTypeAvailabilities.GetByEventIdAsync(order.EventId, cancellationToken);
                foreach (var item in order.OrderItems)
                    ticketTypes.FirstOrDefault(t => t.Id == item.TicketTypeId)?.Release(item.Quantity);

                unitOfWork.Orders.Update(order);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
    }
}
