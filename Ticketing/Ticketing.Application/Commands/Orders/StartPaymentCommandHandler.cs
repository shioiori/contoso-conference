using Eventbox.Shared.Exceptions;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Dtos.Orders;
using MediatR;

namespace Eventbox.Ticketing.Application.Commands.Orders;

public class StartPaymentCommandHandler(IUnitOfWork unitOfWork, TimeProvider timeProvider) : IRequestHandler<StartPaymentCommand, OrderAmountDto>
{
    public async Task<OrderAmountDto> Handle(StartPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Orders.GetByIdWithDetailsAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException("Order", request.OrderId);

        try
        {
            var changed = order.StartPayment(timeProvider.GetUtcNow());
            if (changed)
            {
                unitOfWork.Orders.Update(order);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        var totalAmount = order.OrderItems.Sum(i => i.UnitPrice * i.Quantity);
        var currency = order.OrderItems.FirstOrDefault()?.Currency ?? "VND";

        return new OrderAmountDto { TotalAmount = totalAmount, Currency = currency };
    }
}
