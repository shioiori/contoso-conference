using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Application.Dtos;
using Mapster;
using MediatR;

namespace Eventbox.Ticketing.Application.Queries;

public class GetOrderDetailQueryHandler(
    IOrderRepository orderRepository)
    : IRequestHandler<GetOrderDetailQuery, OrderDto?>
{
    public async Task<OrderDto?> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
    {
        var order = orderRepository
            .Get(x => x.Id == request.OrderId, includeProperties: "OrderItems,Tickets")
            .FirstOrDefault();

        if (order is null)
            return null;

        return new OrderDto
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
            Tickets = order.Tickets
                .OrderBy(ticket => ticket.SequenceNumber)
                .Select(ticket => ticket.Adapt<TicketDto>())
                .ToArray()
        };
    }
}
