using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Ticketing.Application.IntegrationEventHandlers.Events;
using Eventbox.Ticketing.Application.IntegrationEventHandlers.Inventory;
using Eventbox.Ticketing.Application.IntegrationEventHandlers.Orders;

namespace Eventbox.Ticketing.Api.HostedServices;

public sealed class IntegrationEventSubscriptionHostedService(IEventBus eventBus, ILogger<IntegrationEventSubscriptionHostedService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Subscribing Registration integration event handlers.");

        await Task.WhenAll(
            eventBus.SubscribeAsync<
                OrderExpirationDueMessageIntegrationEvent,
                OrderExpirationDueMessageHandler>(stoppingToken),
            eventBus.SubscribeAsync<
                PaymentConfirmedIntegrationEvent,
                PaymentConfirmedIntegrationEventHandler>(stoppingToken),
            eventBus.SubscribeAsync<
                PaymentFailedIntegrationEvent,
                PaymentFailedIntegrationEventHandler>(stoppingToken),
            eventBus.SubscribeAsync<
                EventCreatedIntegrationEvent,
                EventCreatedEventHandler>(stoppingToken),
            eventBus.SubscribeAsync<
                EventUpdatedIntegrationEvent,
                EventUpdatedEventHandler>(stoppingToken),
            eventBus.SubscribeAsync<
                EventPublishedIntegrationEvent,
                EventPublishedEventHandler>(stoppingToken),
            eventBus.SubscribeAsync<
                EventUnpublishedIntegrationEvent,
                EventUnpublishedEventHandler>(stoppingToken),
            eventBus.SubscribeAsync<
                TicketTypeCreatedIntegrationEvent,
                TicketTypeCreatedEventHandler>(stoppingToken),
            eventBus.SubscribeAsync<
                TicketCapacityAddedIntegrationEvent,
                TicketCapacityAddedEventHandler>(stoppingToken),
            eventBus.SubscribeAsync<
                TicketTypeDeletedIntegrationEvent,
                TicketTypeDeletedEventHandler>(stoppingToken));

        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }
}
