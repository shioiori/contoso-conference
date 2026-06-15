using MediatR;
using Eventbox.TicketingApplication.Abstractions;
using Eventbox.TicketingApplication.Abstractions.Repositories;
using Eventbox.Shared.Exceptions;

namespace Eventbox.TicketingApplication.Commands
{
    public class ConfirmOrderCommandHandler(
        IOrderRepository orderRepository,
        IRegistrationUnitOfWork unitOfWork,
        IQrTokenGenerator qrTokenGenerator,
        IQrTokenHasher qrTokenHasher) : IRequestHandler<ConfirmOrderCommand, bool>
    {
        public async Task<bool> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await orderRepository.GetByIdWithDetailsAsync(request.OrderId, cancellationToken)
                ?? throw new NotFoundException("Order", request.OrderId);

            var stateChanged = order.Confirm();
            if (stateChanged)
            {
                foreach (var ticket in order.Tickets)
                {
                    if (!string.IsNullOrWhiteSpace(ticket.QrTokenHash))
                        continue;

                    var qrToken = await GenerateUniqueQrTokenAsync(cancellationToken);
                    ticket.AssignQrToken(qrToken, qrTokenHasher.Hash(qrToken));
                }

                orderRepository.Update(order);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return true;
        }

        private async Task<string> GenerateUniqueQrTokenAsync(CancellationToken cancellationToken)
        {
            string qrToken;
            string qrTokenHash;

            do
            {
                qrToken = qrTokenGenerator.Generate();
                qrTokenHash = qrTokenHasher.Hash(qrToken);
            }
            while (await orderRepository.ExistsTicketByQrTokenHashAsync(qrTokenHash, cancellationToken));

            return qrToken;
        }
    }
}
