using Eventbox.Contracts.IntegrationEvents;
using EventBus.Aws;
using EventBus.RabbitMQ;

namespace Eventbox.Payment.Api.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration["Messaging:Provider"] == "Aws")
        {
            return AwsMessagingExtensions.AddMessaging(services, configuration, options =>
            {
                options.Publish<PaymentConfirmedIntegrationEvent>(configuration["Aws:Topics:Payment"]!);
                options.Publish<PaymentFailedIntegrationEvent>(configuration["Aws:Topics:Payment"]!);
            });
        }

        return RabbitMQMessagingExtensions.AddMessaging(services, configuration, options =>
        {
            options.Publish<PaymentConfirmedIntegrationEvent>("eventbox.payment");
            options.Publish<PaymentFailedIntegrationEvent>("eventbox.payment");
        });
    }
}
