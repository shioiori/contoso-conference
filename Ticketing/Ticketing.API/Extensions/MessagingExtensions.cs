using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using EventBus.RabbitMQ;

namespace Eventbox.Ticketing.Api.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMQOptions>(options =>
        {
            configuration.GetSection("RabbitMQ").Bind(options);
            options.Subscribe<PaymentConfirmedIntegrationEvent>("eventbox.payment");
            options.Subscribe<PaymentFailedIntegrationEvent>("eventbox.payment");
            options.Subscribe<EventCreatedEvent>("eventbox.events");
            options.Subscribe<EventUpdatedEvent>("eventbox.events");
            options.Subscribe<EventPublishedEvent>("eventbox.events");
            options.Subscribe<EventUnpublishedEvent>("eventbox.events");
            options.Subscribe<TicketTypeCreatedEvent>("eventbox.ticketing");
            options.Subscribe<TicketCapacityAddedEvent>("eventbox.ticketing");
            options.Subscribe<TicketTypeDeletedEvent>("eventbox.ticketing");
            options.Subscribe<OrderExpirationDueMessageIntegrationEvent>(
                "eventbox.ticketing",
                routingKey: "ticketing.expire",
                queue: "eventbox.ticketing.expire");
            options.Publish<OrderConfirmedIntegrationEvent>("eventbox.ticketing");
        });

        services.AddSingleton<RabbitMQEventBus>();
        services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<RabbitMQEventBus>());
        services.AddSingleton<IDelayedEventScheduler>(sp => sp.GetRequiredService<RabbitMQEventBus>());
        services.AddHostedService(sp => sp.GetRequiredService<RabbitMQEventBus>());

        return services;
    }
}
