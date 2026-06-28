using Eventbox.EventBus.Core.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.RabbitMQ;

public static class RabbitMQMessagingExtensions
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<RabbitMQOptions> configure,
        bool includeDelayedScheduler = false)
    {
        services.Configure<RabbitMQOptions>(options =>
        {
            configuration.GetSection("RabbitMQ").Bind(options);
            configure(options);
        });

        services.AddSingleton<RabbitMQEventBus>();
        services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<RabbitMQEventBus>());
        services.AddHostedService(sp => sp.GetRequiredService<RabbitMQEventBus>());

        if (includeDelayedScheduler)
            services.AddSingleton<IDelayedEventScheduler>(sp => sp.GetRequiredService<RabbitMQEventBus>());

        return services;
    }
}
