using Contoso.ServiceBus.Abstractions;
using Contoso.ServiceBus.RabbitMQ;
using Contoso.ServiceBus.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Contoso.ServiceBus.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMqServiceBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(
            configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddSingleton<RabbitMqConnectionFactory>();
        services.AddSingleton<SubscriptionRegistry>();
        services.AddSingleton<IMessageSerializer, NewtonsoftJsonMessageSerializer>();
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

    public static IServiceCollection AddCommandHandler<TCommand, THandler>(
        this IServiceCollection services)
        where TCommand : ICommand
        where THandler : class, ICommandHandler<TCommand>
    {
        services.AddScoped<THandler>();
        return services;
    }
}
