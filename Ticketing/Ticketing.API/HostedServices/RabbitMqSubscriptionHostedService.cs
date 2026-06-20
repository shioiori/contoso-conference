using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Ticketing.Application.IntegrationEventHandlers;
using EventPublishedEventHandler = Eventbox.Ticketing.Application.IntegrationEventHandlers.EventPublishedEventHandler;
using EventUnpublishedEventHandler = Eventbox.Ticketing.Application.IntegrationEventHandlers.EventUnpublishedEventHandler;

namespace Eventbox.Ticketing.Api.HostedServices;

public sealed class RabbitMqSubscriptionHostedService : BackgroundService
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<RabbitMqSubscriptionHostedService> _logger;

    public RabbitMqSubscriptionHostedService(
        IEventBus eventBus,
        ILogger<RabbitMqSubscriptionHostedService> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Subscribing Registration integration event handlers.");

        await _eventBus.SubscribeAsync<
            OrderExpirationDueMessageIntergrationEvent,
            OrderExpirationDueMessageHandler>(stoppingToken);

        await _eventBus.SubscribeAsync<
            PaymentConfirmedIntegrationEvent,
            PaymentConfirmedIntegrationEventHandler>(stoppingToken);

        await _eventBus.SubscribeAsync<
            PaymentFailedIntegrationEvent,
            PaymentFailedIntegrationEventHandler>(stoppingToken);

        await _eventBus.SubscribeAsync<
            EventCreatedEvent,
            EventCreatedEventHandler>(stoppingToken);

        await _eventBus.SubscribeAsync<
            EventUpdatedEvent,
            EventUpdatedEventHandler>(stoppingToken);

        await _eventBus.SubscribeAsync<
            TicketTypeCreatedEvent,
            TicketTypeCreatedEventHandler>(stoppingToken);

        await _eventBus.SubscribeAsync<
            TicketCapacityAddedEvent,
            TicketCapacityAddedEventHandler>(stoppingToken);

        await _eventBus.SubscribeAsync<
            TicketTypeDeletedEvent,
            TicketTypeDeletedEventHandler>(stoppingToken);

        await _eventBus.SubscribeAsync<
            EventPublishedEvent,
            EventPublishedEventHandler>(stoppingToken);

        await _eventBus.SubscribeAsync<
            EventUnpublishedEvent,
            EventUnpublishedEventHandler>(stoppingToken);

        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }
}
