using Eventbox.Contracts.IntegrationEvents;
using EventBus.Aws;
using EventBus.RabbitMQ;

namespace Eventbox.EventManagement.EventApi.Api.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration["Messaging:Provider"] == "Aws")
        {
            return AwsMessagingExtensions.AddMessaging(services, configuration, options =>
            {
                options.Publish<EventCreatedIntegrationEvent>(configuration["Aws:Topics:Events"]!);
                options.Publish<EventUpdatedIntegrationEvent>(configuration["Aws:Topics:Events"]!);
                options.Publish<EventPublishedIntegrationEvent>(configuration["Aws:Topics:Events"]!);
                options.Publish<EventUnpublishedIntegrationEvent>(configuration["Aws:Topics:Events"]!);
                options.Publish<TicketTypeCreatedIntegrationEvent>(configuration["Aws:Topics:Ticketing"]!);
                options.Publish<TicketCapacityAddedIntegrationEvent>(configuration["Aws:Topics:Ticketing"]!);
                options.Publish<TicketTypeDeletedIntegrationEvent>(configuration["Aws:Topics:Ticketing"]!);
            });
        }

        return RabbitMQMessagingExtensions.AddMessaging(services, configuration, options =>
        {
            options.Publish<EventCreatedIntegrationEvent>("eventbox.events");
            options.Publish<EventUpdatedIntegrationEvent>("eventbox.events");
            options.Publish<EventPublishedIntegrationEvent>("eventbox.events");
            options.Publish<EventUnpublishedIntegrationEvent>("eventbox.events");
            options.Publish<TicketTypeCreatedIntegrationEvent>("eventbox.ticketing");
            options.Publish<TicketCapacityAddedIntegrationEvent>("eventbox.ticketing");
            options.Publish<TicketTypeDeletedIntegrationEvent>("eventbox.ticketing");
        });
    }
}
