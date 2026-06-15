using MediatR;
using Eventbox.Registration.Application.Abstractions;

namespace Eventbox.Registration.Application.Commands
{
    public class ConfirmOrderCommandHandler(
        IUnitOfWork unitOfWork) : IRequestHandler<ConfirmOrderCommand, bool>
    {
        public async Task<bool> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken)
                ?? throw new KeyNotFoundException($"Order '{request.OrderId}' was not found.");

            var stateChanged = order.Confirm();
            if (stateChanged)
            {
                unitOfWork.Orders.Update(order);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
    }
}
