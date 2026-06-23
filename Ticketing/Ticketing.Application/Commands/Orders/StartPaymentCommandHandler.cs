using Eventbox.Shared.Exceptions;
using Eventbox.Ticketing.Application.Abstractions;
using MediatR;

namespace Eventbox.Ticketing.Application.Commands.Orders;

public class StartPaymentCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<StartPaymentCommand>
{
    public async Task Handle(StartPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException("Order", request.OrderId);

        try
        {
            var changed = order.StartPayment();
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
    }
}
