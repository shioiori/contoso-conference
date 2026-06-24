using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Ticketing.Application.IntegrationEventHandlers.Events;
using Eventbox.Ticketing.Application.IntegrationEventHandlers.Inventory;
using Eventbox.Ticketing.Application.IntegrationEventHandlers.Orders;

namespace Eventbox.Ticketing.Api.HostedServices;

public sealed class RabbitMqSubscriptionHostedService(IEventBus eventBus, ILogger<RabbitMqSubscriptionHostedService> logger) 
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Subscribing Registration integration event handlers.");

        await eventBus.SubscribeAsync<
            OrderExpirationDueMessageIntegrationEvent,
            OrderExpirationDueMessageHandler>(stoppingToken);

        await eventBus.SubscribeAsync<
            PaymentConfirmedIntegrationEvent,
            PaymentConfirmedIntegrationEventHandler>(stoppingToken);

        await eventBus.SubscribeAsync<
            PaymentFailedIntegrationEvent,
            PaymentFailedIntegrationEventHandler>(stoppingToken);

        await eventBus.SubscribeAsync<
            EventCreatedEvent,
            EventCreatedEventHandler>(stoppingToken);

        await eventBus.SubscribeAsync<
            EventUpdatedEvent,
            EventUpdatedEventHandler>(stoppingToken);

        await eventBus.SubscribeAsync<
            EventPublishedEvent,
            EventPublishedEventHandler>(stoppingToken);

        await eventBus.SubscribeAsync<
            EventUnpublishedEvent,
            EventUnpublishedEventHandler>(stoppingToken);

        await eventBus.SubscribeAsync<
            TicketTypeCreatedEvent,
            TicketTypeCreatedEventHandler>(stoppingToken);

        await eventBus.SubscribeAsync<
            TicketCapacityAddedEvent,
            TicketCapacityAddedEventHandler>(stoppingToken);

        await eventBus.SubscribeAsync<
            TicketTypeDeletedEvent,
            TicketTypeDeletedEventHandler>(stoppingToken);


        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }
}
