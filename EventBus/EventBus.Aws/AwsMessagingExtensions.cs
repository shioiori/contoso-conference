using Eventbox.EventBus.Core.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.Aws
{
    public static class AwsMessagingExtensions
    {
        public static IServiceCollection AddMessaging(this IServiceCollection services, 
            IConfiguration configuration, 
            Action<AwsOptions> configure,
            bool includeDelayedScheduler = false)
        {
            services.Configure<AwsOptions>(options =>
            {
                configuration.GetSection("Aws").Bind(options);
                configure(options);
            });
            services.AddSingleton<AwsEventBus>();
            services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<AwsEventBus>());
            services.AddHostedService(sp => sp.GetRequiredService<AwsEventBus>());
            if (includeDelayedScheduler)
                services.AddSingleton<IDelayedEventScheduler>(sp => sp.GetRequiredService<AwsEventBus>());
            return services;
        }
    }
}
