using Eventbox.EventBus.Core.Abstractions;
using Eventbox.TicketingApplication.Abstractions;
using Eventbox.TicketingApplication.Abstractions.Repositories;
using Eventbox.TicketingApplication.IntegrationEvents;

namespace Eventbox.TicketingApplication.IntegrationEventHandlers;

public class EventUpdatedEventHandler(
    IEventScheduleRepository eventScheduleRepository,
    IRegistrationUnitOfWork unitOfWork) : IIntegrationEventHandler<EventUpdatedEvent>
{
    public async Task HandleAsync(EventUpdatedEvent @event, CancellationToken cancellationToken = default)
    {
        await eventScheduleRepository.UpsertAsync(@event.EventId, @event.From, @event.To, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
