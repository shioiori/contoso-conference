namespace Eventbox.Ticketing.Application.Abstractions.Jobs;

public interface IOrderExpirationScheduler
{
    Task ScheduleExpirationAsync(
        Guid orderId,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken = default);
}
