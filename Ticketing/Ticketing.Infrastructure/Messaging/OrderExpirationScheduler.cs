using Eventbox.EventBus.Core.Abstractions;
using Eventbox.TicketingApplication.Abstractions.Jobs;
using Eventbox.TicketingApplication.Messages;

namespace Eventbox.TicketingInfrastructure.Messaging;

public sealed class OrderExpirationScheduler : IOrderExpirationScheduler
{
    private readonly IDelayedEventScheduler _delayedEventScheduler;

    public OrderExpirationScheduler(IDelayedEventScheduler delayedEventScheduler)
    {
        _delayedEventScheduler = delayedEventScheduler;
    }

    public Task ScheduleExpirationAsync(
        Guid orderId,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken = default)
    {
        return _delayedEventScheduler.ScheduleAsync(
            new OrderExpirationDueMessage(orderId, expiresAt),
            expiresAt,
            cancellationToken);
    }
}
