using Eventbox.Contracts.IntegrationEvents;
using EventBus.Aws;
using EventBus.RabbitMQ;

namespace Eventbox.Notification.Api.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration["Messaging:Provider"] == "Aws")
        {
            return AwsMessagingExtensions.AddMessaging(services, configuration, options =>
            {
                options.Subscribe<OrderConfirmedIntegrationEvent>(
                    configuration["Aws:Queues:Notification"]!,
                    configuration["Aws:Queues:NotificationDlq"]!);
            });
        }

        return RabbitMQMessagingExtensions.AddMessaging(services, configuration, options =>
        {
            options.Subscribe<OrderConfirmedIntegrationEvent>("eventbox.ticketing");
        });
    }
}
