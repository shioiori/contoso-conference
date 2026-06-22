using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;

namespace Eventbox.Ticketing.Application.IntegrationEventHandlers.Orders;

public class PaymentFailedIntegrationEventHandler(
    IUnitOfWork unitOfWork) : IIntegrationEventHandler<PaymentFailedIntegrationEvent>
{
    public async Task HandleAsync(PaymentFailedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        var order = await unitOfWork.Orders.GetByIdWithDetailsAsync(@event.OrderId, cancellationToken);
        if (order is null)
            return;

        var stateChanged = order.MarkPaymentFailed();
        if (stateChanged)
        {
            unitOfWork.Orders.Update(order);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
