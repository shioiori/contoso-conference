using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Dtos.Orders;
using Eventbox.Ticketing.Application.Dtos.Tickets;
using Mapster;
using MediatR;

namespace Eventbox.Ticketing.Application.Queries.Orders;

public class GetOrdersByEmailQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetOrdersByEmailQuery, IEnumerable<OrderDto>>
{
    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersByEmailQuery request, CancellationToken cancellationToken)
    {
        var orders = (await unitOfWork.Orders.GetByEmailAsync(request.Email, cancellationToken)).ToList();
        if (orders.Count == 0)
            return [];

        var orderIds = orders.Select(o => o.Id).ToList();
        var allTickets = await unitOfWork.Tickets.GetByOrderIdsAsync(orderIds, cancellationToken);
        var ticketsByOrder = allTickets.GroupBy(t => t.OrderId).ToDictionary(g => g.Key, g => g.ToList());

        return orders.Select(order => new OrderDto
        {
            Id = order.Id,
            EventId = order.EventId,
            UserId = order.UserId,
            OrderState = order.GetCurrentState(DateTimeOffset.UtcNow),
            AccessCode = order.AccessCode,
            ReservationExpiresAt = order.ReservationExpiresAt,
            Name = order.PersonalInfo.Name,
            Email = order.PersonalInfo.Email,
            Items = order.OrderItems.Adapt<IReadOnlyCollection<OrderItemDto>>(),
            Tickets = ticketsByOrder.TryGetValue(order.Id, out var tickets)
                ? tickets.OrderBy(t => t.SequenceNumber).Select(t => t.Adapt<TicketDto>()).ToArray()
                : []
        });
    }
}
