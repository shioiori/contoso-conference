using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Jobs;
using Eventbox.Ticketing.Application.Messages;

namespace Eventbox.Ticketing.Infrastructure.Messaging;

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
