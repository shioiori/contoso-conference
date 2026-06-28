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

        var exchanges = configuration.GetSection("RabbitMQ:Exchanges");
        return RabbitMQMessagingExtensions.AddMessaging(services, configuration, options =>
        {
            options.Publish<EventCreatedIntegrationEvent>(exchanges["Events"]!);
            options.Publish<EventUpdatedIntegrationEvent>(exchanges["Events"]!);
            options.Publish<EventPublishedIntegrationEvent>(exchanges["Events"]!);
            options.Publish<EventUnpublishedIntegrationEvent>(exchanges["Events"]!);
            options.Publish<TicketTypeCreatedIntegrationEvent>(exchanges["Ticketing"]!);
            options.Publish<TicketCapacityAddedIntegrationEvent>(exchanges["Ticketing"]!);
            options.Publish<TicketTypeDeletedIntegrationEvent>(exchanges["Ticketing"]!);
        });
    }
}
