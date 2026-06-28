using Eventbox.Contracts.IntegrationEvents;
using EventBus.RabbitMQ;

namespace Eventbox.Ticketing.Api.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddMessaging(configuration, options =>
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
