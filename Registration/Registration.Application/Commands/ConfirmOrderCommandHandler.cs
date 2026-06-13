using MediatR;
using Eventbox.Registration.Application.Abstractions;
using Eventbox.Registration.Application.Abstractions.Repositories;

namespace Eventbox.Registration.Application.Commands
{
    public class ConfirmOrderCommandHandler(
        IOrderRepository orderRepository,
        IRegistrationUnitOfWork unitOfWork) : IRequestHandler<ConfirmOrderCommand, bool>
    {
        public async Task<bool> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
                ?? throw new KeyNotFoundException($"Order '{request.OrderId}' was not found.");

            var stateChanged = order.Confirm();
            if (stateChanged)
            {
                orderRepository.Update(order);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
    }
}
