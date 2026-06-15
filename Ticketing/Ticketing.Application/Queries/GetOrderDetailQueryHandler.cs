using Eventbox.TicketingApplication.Abstractions.Repositories;
using Eventbox.TicketingApplication.Dtos;
using Mapster;
using MediatR;

namespace Eventbox.TicketingApplication.Queries;

public class GetOrderDetailQueryHandler(
    IOrderRepository orderRepository,
    ICheckInPassRepository checkInPassRepository)
    : IRequestHandler<GetOrderDetailQuery, OrderDto?>
{
    public async Task<OrderDto?> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
    {
        var order = orderRepository
            .Get(x => x.Id == request.OrderId, includeProperties: "OrderItems,Tickets")
            .FirstOrDefault();

        if (order is null)
            return null;

        var checkInPasses = await checkInPassRepository.GetByRegistrationOrderIdAsync(order.Id, cancellationToken);
        var passesByTicketId = checkInPasses.ToDictionary(pass => pass.RegistrationTicketId);

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
                .Select(ticket =>
                {
                    passesByTicketId.TryGetValue(ticket.Id, out var pass);
                    return new TicketDto
                    {
                        Id = ticket.Id,
                        TicketTypeId = ticket.TicketTypeId,
                        SequenceNumber = ticket.SequenceNumber,
                        TicketState = ticket.TicketState,
                        QrToken = pass?.QrToken,
                        CheckInStatus = pass?.Status,
                        CheckedInAt = pass?.CheckedInAt
                    };
                })
                .ToArray()
        };
    }
}
