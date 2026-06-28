using Eventbox.Contracts.IntegrationEvents;
using EventBus.Aws;
using EventBus.RabbitMQ;

namespace Eventbox.Ticketing.Api.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration["Messaging:Provider"] == "Aws")
        {
            return AwsMessagingExtensions.AddMessaging(services, configuration, options =>
            {
                options.Subscribe<PaymentConfirmedIntegrationEvent>(
                    configuration["Aws:Queues:Ticketing"]!,
                    configuration["Aws:Queues:TicketingDlq"]!);
                options.Subscribe<PaymentFailedIntegrationEvent>(
                    configuration["Aws:Queues:Ticketing"]!,
                    configuration["Aws:Queues:TicketingDlq"]!);
                options.Subscribe<EventCreatedIntegrationEvent>(
                    configuration["Aws:Queues:Ticketing"]!,
                    configuration["Aws:Queues:TicketingDlq"]!);
                options.Subscribe<EventUpdatedIntegrationEvent>(
                    configuration["Aws:Queues:Ticketing"]!,
                    configuration["Aws:Queues:TicketingDlq"]!);
                options.Subscribe<EventPublishedIntegrationEvent>(
                    configuration["Aws:Queues:Ticketing"]!,
                    configuration["Aws:Queues:TicketingDlq"]!);
                options.Subscribe<EventUnpublishedIntegrationEvent>(
                    configuration["Aws:Queues:Ticketing"]!,
                    configuration["Aws:Queues:TicketingDlq"]!);
                options.Subscribe<TicketTypeCreatedIntegrationEvent>(
                    configuration["Aws:Queues:Ticketing"]!,
                    configuration["Aws:Queues:TicketingDlq"]!);
                options.Subscribe<TicketCapacityAddedIntegrationEvent>(
                    configuration["Aws:Queues:Ticketing"]!,
                    configuration["Aws:Queues:TicketingDlq"]!);
                options.Subscribe<TicketTypeDeletedIntegrationEvent>(
                    configuration["Aws:Queues:Ticketing"]!,
                    configuration["Aws:Queues:TicketingDlq"]!);
                options.Subscribe<OrderExpirationDueMessageIntegrationEvent>(
                    configuration["Aws:Queues:TicketingExpiration"]!,
                    configuration["Aws:Queues:TicketingExpirationDlq"]!);
                options.Publish<OrderConfirmedIntegrationEvent>(configuration["Aws:Topics:Ticketing"]!);
            }, includeDelayedScheduler: true);
        }

        var exchanges = configuration.GetSection("RabbitMQ:Exchanges");
        return RabbitMQMessagingExtensions.AddMessaging(services, configuration, options =>
        {
            options.Subscribe<PaymentConfirmedIntegrationEvent>(exchanges["Payment"]!);
            options.Subscribe<PaymentFailedIntegrationEvent>(exchanges["Payment"]!);
            options.Subscribe<EventCreatedIntegrationEvent>(exchanges["Events"]!);
            options.Subscribe<EventUpdatedIntegrationEvent>(exchanges["Events"]!);
            options.Subscribe<EventPublishedIntegrationEvent>(exchanges["Events"]!);
            options.Subscribe<EventUnpublishedIntegrationEvent>(exchanges["Events"]!);
            options.Subscribe<TicketTypeCreatedIntegrationEvent>(exchanges["Ticketing"]!);
            options.Subscribe<TicketCapacityAddedIntegrationEvent>(exchanges["Ticketing"]!);
            options.Subscribe<TicketTypeDeletedIntegrationEvent>(exchanges["Ticketing"]!);
            options.Subscribe<OrderExpirationDueMessageIntegrationEvent>(
                exchanges["Ticketing"]!,
                routingKey: "ticketing.expire",
                queue: "eventbox.ticketing.expire");
            options.Publish<OrderConfirmedIntegrationEvent>(exchanges["Ticketing"]!);
        }, includeDelayedScheduler: true);
    }
}
