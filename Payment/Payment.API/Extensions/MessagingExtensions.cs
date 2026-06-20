using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using EventBus.RabbitMQ;

namespace Eventbox.Payment.Api.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMQOptions>(options =>
        {
            configuration.GetSection("RabbitMQ").Bind(options);
            options.Publish<PaymentConfirmedIntegrationEvent>("eventbox.payment");
            options.Publish<PaymentFailedIntegrationEvent>("eventbox.payment");
        });

        services.AddSingleton<RabbitMQEventBus>();
        services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<RabbitMQEventBus>());
        services.AddHostedService(sp => sp.GetRequiredService<RabbitMQEventBus>());

        return services;
    }
}
