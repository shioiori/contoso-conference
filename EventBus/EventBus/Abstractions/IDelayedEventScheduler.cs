namespace Eventbox.EventBus.Core.Abstractions;

public interface IDelayedEventScheduler
{
    Task ScheduleAsync<TEvent>(
        TEvent @event,
        DateTimeOffset deliverAt,
        CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;
}
