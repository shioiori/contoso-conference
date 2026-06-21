using Eventbox.Shared.Exceptions;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Domain.Tickets;
using MediatR;

namespace Eventbox.Ticketing.Application.Commands;

public class ConfirmOrderCommandHandler(
    IUnitOfWork unitOfWork,
    IQrTokenGenerator qrTokenGenerator,
    IQrTokenHasher qrTokenHasher) : IRequestHandler<ConfirmOrderCommand, bool>
{
    public async Task<bool> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Orders.GetByIdWithDetailsAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException("Order", request.OrderId);

        var stateChanged = order.Confirm();
        if (stateChanged)
        {
            var tickets = new List<Ticket>();
            int sequenceNumber = 1;

            foreach (var item in order.OrderItems)
            {
                for (int i = 0; i < item.Quantity; i++)
                {
                    var (qrToken, qrTokenHash) = await GenerateUniqueQrTokenAsync(cancellationToken);
                    var ticket = new Ticket(order.Id, order.EventId, item.TicketTypeId, sequenceNumber++);
                    ticket.AssignQrToken(qrToken, qrTokenHash);
                    tickets.Add(ticket);
                }
            }

            await unitOfWork.Tickets.AddRangeAsync(tickets, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    private async Task<(string, string)> GenerateUniqueQrTokenAsync(CancellationToken cancellationToken)
    {
        string qrToken;
        string qrTokenHash;

        do
        {
            qrToken = qrTokenGenerator.Generate();
            qrTokenHash = qrTokenHasher.Hash(qrToken);
        }
        while (await unitOfWork.Tickets.ExistsByQrTokenHashAsync(qrTokenHash, cancellationToken));

        return (qrToken, qrTokenHash);
    }
}
