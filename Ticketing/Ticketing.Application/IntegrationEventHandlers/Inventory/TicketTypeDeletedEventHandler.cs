using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;

namespace Eventbox.Ticketing.Application.IntegrationEventHandlers.Inventory;

public class TicketTypeDeletedEventHandler(
    IUnitOfWork unitOfWork) : IIntegrationEventHandler<TicketTypeDeletedEvent>
{
    public async Task HandleAsync(TicketTypeDeletedEvent @event, CancellationToken cancellationToken = default)
    {
        var availability = await unitOfWork.TicketTypeAvailabilities.GetByIdAsync(@event.Id, cancellationToken);
        if (availability == null)
            return;
        unitOfWork.TicketTypeAvailabilities.Delete(availability);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
