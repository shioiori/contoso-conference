using Eventbox.EventBus.Core.Abstractions;
using Eventbox.TicketingApplication.IntegrationEventHandlers;
using Eventbox.TicketingApplication.IntegrationEvents;
using Eventbox.TicketingApplication.MessageHandlers;
using Eventbox.TicketingApplication.Messages;

namespace Eventbox.TicketingApi.HostedServices;

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
            OrderExpirationDueMessage,
            OrderExpirationDueMessageHandler>(stoppingToken);

        await _eventBus.SubscribeAsync<
            PaymentConfirmedIntegrationEvent,
            PaymentConfirmedIntegrationEventHandler>(stoppingToken);

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

        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }
}
