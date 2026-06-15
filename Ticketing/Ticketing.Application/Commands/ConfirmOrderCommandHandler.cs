using MediatR;
using Eventbox.TicketingApplication.Abstractions;
using Eventbox.TicketingApplication.Abstractions.Repositories;
using Eventbox.TicketingDomain.Entities.CheckInAggregate;
using Eventbox.Shared.Exceptions;

namespace Eventbox.TicketingApplication.Commands
{
    public class ConfirmOrderCommandHandler(
        IOrderRepository orderRepository,
        ICheckInPassRepository checkInPassRepository,
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
                    if (await checkInPassRepository.ExistsByRegistrationTicketIdAsync(ticket.Id, cancellationToken))
                        continue;

                    var qrToken = await GenerateUniqueQrTokenAsync(cancellationToken);
                    var attendeeName = string.IsNullOrWhiteSpace(order.PersonalInfo.Name)
                        ? order.PersonalInfo.Email
                        : order.PersonalInfo.Name;

                    await checkInPassRepository.AddAsync(
                        new CheckInPass(
                            order.EventId,
                            order.Id,
                            ticket.Id,
                            ticket.TicketTypeId,
                            attendeeName,
                            order.PersonalInfo.Email,
                            qrToken,
                            qrTokenHasher.Hash(qrToken)),
                        cancellationToken);
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
            while (await checkInPassRepository.ExistsByQrTokenHashAsync(qrTokenHash, cancellationToken));

            return qrToken;
        }
    }
}
