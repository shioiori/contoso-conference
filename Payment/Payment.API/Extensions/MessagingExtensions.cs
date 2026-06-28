using Eventbox.Contracts.IntegrationEvents;
using EventBus.RabbitMQ;

namespace Eventbox.Payment.Api.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddMessaging(configuration, options =>
        {
            options.Publish<PaymentConfirmedIntegrationEvent>("eventbox.payment");
            options.Publish<PaymentFailedIntegrationEvent>("eventbox.payment");
        });
    }
}
