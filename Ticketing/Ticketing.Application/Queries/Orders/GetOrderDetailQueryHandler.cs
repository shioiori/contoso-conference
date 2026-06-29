using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Dtos.Orders;
using Eventbox.Ticketing.Application.Dtos.Tickets;
using Mapster;
using MediatR;

namespace Eventbox.Ticketing.Application.Queries.Orders;

public class GetOrderDetailQueryHandler(
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<GetOrderDetailQuery, OrderDto>
{
    public async Task<OrderDto> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
    {
        var order = unitOfWork.Orders
            .Get(x => x.Id == request.OrderId, includeProperties: "OrderItems")
            .FirstOrDefault();

        if (order is null)
            return null;

        var tickets = await unitOfWork.Tickets.GetByOrderIdAsync(order.Id, cancellationToken);

        return new OrderDto
        {
            Id = order.Id,
            EventId = order.EventId,
            UserId = order.UserId,
            OrderState = order.GetCurrentState(timeProvider.GetUtcNow()),
            AccessCode = order.AccessCode,
            ReservationExpiresAt = order.ReservationExpiresAt,
            Name = order.PersonalInfo.Name,
            Email = order.PersonalInfo.Email,
            Items = order.OrderItems.Adapt<IReadOnlyCollection<OrderItemDto>>(),
            Tickets = tickets
                .OrderBy(t => t.SequenceNumber)
                .Select(t => t.Adapt<TicketDto>())
                .ToArray()
        };
    }
}
