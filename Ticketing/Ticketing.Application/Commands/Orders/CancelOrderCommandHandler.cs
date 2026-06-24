using Eventbox.Shared.Exceptions;
using Eventbox.Ticketing.Application.Abstractions;
using MediatR;

namespace Eventbox.Ticketing.Application.Commands.Orders;

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
            ?? throw new NotFoundException("Order", request.OrderId);

        var stateChanged = order.Cancel();

        if (stateChanged)
        {
            var tickets = await unitOfWork.Tickets.GetByOrderIdAsync(order.Id, cancellationToken);
            foreach (var ticket in tickets)
                ticket.Cancel();

            var ticketTypes = await unitOfWork.TicketTypeAvailabilities.GetByEventIdAsync(order.EventId, cancellationToken);
            foreach (var item in order.OrderItems)
                ticketTypes.FirstOrDefault(t => t.Id == item.TicketTypeId)?.Release(item.Quantity);

            unitOfWork.Orders.Update(order);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}
