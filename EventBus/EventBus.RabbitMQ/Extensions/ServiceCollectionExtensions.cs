using Contoso.EventBus.Abstractions;
using Contoso.EventBus.RabbitMQ;
using Contoso.EventBus.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Contoso.EventBus.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMqEventBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(
            configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddSingleton<RabbitMqConnectionFactory>();
        services.AddSingleton<SubscriptionRegistry>();
        services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
        services.AddSingleton<IEventBus, RabbitMqEventBus>();
        services.AddSingleton<ICommandBus, RabbitMqCommandBus>();

        return services;
    }

    public static IServiceCollection AddIntegrationEventHandler<TEvent, THandler>(
        this IServiceCollection services)
        where TEvent : IIntegrationEvent
        where THandler : class, IIntegrationEventHandler<TEvent>
    {
        services.AddScoped<THandler>();
        return services;
    }
}
