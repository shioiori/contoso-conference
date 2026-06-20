using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using EventBus.RabbitMQ;

namespace Eventbox.EventManagement.EventApi.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMQOptions>(options =>
        {
            configuration.GetSection("RabbitMQ").Bind(options);
            options.Publish<EventCreatedEvent>("eventbox.events");
            options.Publish<EventUpdatedEvent>("eventbox.events");
            options.Publish<EventPublishedEvent>("eventbox.events");
            options.Publish<EventUnpublishedEvent>("eventbox.events");
            options.Publish<TicketTypeCreatedEvent>("eventbox.ticketing");
            options.Publish<TicketCapacityAddedEvent>("eventbox.ticketing");
            options.Publish<TicketTypeDeletedEvent>("eventbox.ticketing");
        });

        services.AddSingleton<RabbitMQEventBus>();
        services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<RabbitMQEventBus>());
        services.AddHostedService(sp => sp.GetRequiredService<RabbitMQEventBus>());

        return services;
    }
}
