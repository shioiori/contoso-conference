using Eventbox.Contracts.IntegrationEvents;
using EventBus.RabbitMQ;

namespace Eventbox.Notification.Api.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddMessaging(configuration, options =>
        {
            options.Subscribe<OrderConfirmedIntegrationEvent>("eventbox.ticketing");
        });
    }
}
