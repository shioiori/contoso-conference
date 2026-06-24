using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using EventBus.RabbitMQ;

namespace Eventbox.EventManagement.EventApi.Api.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMQOptions>(options =>
        {
            configuration.GetSection("RabbitMQ").Bind(options);
            options.Publish<EventCreatedIntegrationEvent>("eventbox.events");
            options.Publish<EventUpdatedIntegrationEvent>("eventbox.events");
            options.Publish<EventPublishedIntegrationEvent>("eventbox.events");
            options.Publish<EventUnpublishedIntegrationEvent>("eventbox.events");
            options.Publish<TicketTypeCreatedIntegrationEvent>("eventbox.ticketing");
            options.Publish<TicketCapacityAddedIntegrationEvent>("eventbox.ticketing");
            options.Publish<TicketTypeDeletedIntegrationEvent>("eventbox.ticketing");
        });

        services.AddSingleton<RabbitMQEventBus>();
        services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<RabbitMQEventBus>());
        services.AddHostedService(sp => sp.GetRequiredService<RabbitMQEventBus>());

        return services;
    }
}
