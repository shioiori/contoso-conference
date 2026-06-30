using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Notification.Api.IntegrationEvents.EventHandlers;

namespace Eventbox.Notification.Api.HostedServices;

public sealed class IntegrationEventSubscriptionHostedService(IEventBus eventBus, ILogger<IntegrationEventSubscriptionHostedService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Subscribing notification integration event handlers.");

        await eventBus.SubscribeAsync<OrderConfirmedIntegrationEvent, OrderConfirmedIntegrationEventHandler>(stoppingToken);

        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }
}
