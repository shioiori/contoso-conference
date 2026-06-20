using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Contracts.IntegrationEvents;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Repositories;

namespace Eventbox.Ticketing.Application.IntegrationEventHandlers;

public class EventCreatedEventHandler(
    IEventScheduleRepository eventScheduleRepository,
    IUnitOfWork unitOfWork) : IIntegrationEventHandler<EventCreatedEvent>
{
    public async Task HandleAsync(EventCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        await eventScheduleRepository.UpsertAsync(@event.EventId, @event.From, @event.To, @event.IsPublished, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
