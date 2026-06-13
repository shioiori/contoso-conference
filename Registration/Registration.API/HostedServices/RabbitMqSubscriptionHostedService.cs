using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Registration.Application.IntegrationEventHandlers;
using Eventbox.Registration.Application.IntegrationEvents;
using Eventbox.Registration.Application.MessageHandlers;
using Eventbox.Registration.Application.Messages;

namespace Eventbox.Registration.Api.HostedServices;

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

        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }
}
