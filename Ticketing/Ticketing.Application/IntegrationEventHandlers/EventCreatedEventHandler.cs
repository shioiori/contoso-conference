using Eventbox.EventBus.Core.Abstractions;
using Eventbox.TicketingApplication.Abstractions;
using Eventbox.TicketingApplication.Abstractions.Repositories;
using Eventbox.TicketingApplication.IntegrationEvents;

namespace Eventbox.TicketingApplication.IntegrationEventHandlers;

public class EventCreatedEventHandler(
    IEventScheduleRepository eventScheduleRepository,
    IRegistrationUnitOfWork unitOfWork) : IIntegrationEventHandler<EventCreatedEvent>
{
    public async Task HandleAsync(EventCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        await eventScheduleRepository.UpsertAsync(@event.EventId, @event.From, @event.To, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
