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

        return RabbitMQMessagingExtensions.AddMessaging(services, configuration, options =>
        {
            options.Subscribe<PaymentConfirmedIntegrationEvent>("eventbox.payment");
            options.Subscribe<PaymentFailedIntegrationEvent>("eventbox.payment");
            options.Subscribe<EventCreatedIntegrationEvent>("eventbox.events");
            options.Subscribe<EventUpdatedIntegrationEvent>("eventbox.events");
            options.Subscribe<EventPublishedIntegrationEvent>("eventbox.events");
            options.Subscribe<EventUnpublishedIntegrationEvent>("eventbox.events");
            options.Subscribe<TicketTypeCreatedIntegrationEvent>("eventbox.ticketing");
            options.Subscribe<TicketCapacityAddedIntegrationEvent>("eventbox.ticketing");
            options.Subscribe<TicketTypeDeletedIntegrationEvent>("eventbox.ticketing");
            options.Subscribe<OrderExpirationDueMessageIntegrationEvent>(
                "eventbox.ticketing",
                routingKey: "ticketing.expire",
                queue: "eventbox.ticketing.expire");
            options.Publish<OrderConfirmedIntegrationEvent>("eventbox.ticketing");
        }, includeDelayedScheduler: true);
    }
}
