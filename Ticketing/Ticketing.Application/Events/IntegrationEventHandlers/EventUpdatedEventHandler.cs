using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Application.IntegrationEvents;

namespace Eventbox.Ticketing.Application.IntegrationEventHandlers;

public class EventUpdatedEventHandler(
    IEventScheduleRepository eventScheduleRepository,
    IUnitOfWork unitOfWork) : IIntegrationEventHandler<EventUpdatedEvent>
{
    public async Task HandleAsync(EventUpdatedEvent @event, CancellationToken cancellationToken = default)
    {
        await eventScheduleRepository.UpsertAsync(@event.EventId, @event.From, @event.To, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
