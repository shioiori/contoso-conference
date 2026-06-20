using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Jobs;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Application.Commands;
using Eventbox.Ticketing.Application.IntegrationEventHandlers;
using Eventbox.Ticketing.Infrastructure;
using Eventbox.Ticketing.Infrastructure.Jobs;
using Eventbox.Ticketing.Infrastructure.Repositories;
using Eventbox.Ticketing.Infrastructure.Security;
using Eventbox.Shared.Outbox;

namespace Eventbox.Ticketing.Api.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<RegisterToEventCommand>());
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ITicketAvailabilityRepository, TicketAvailabilityRepository>();
        services.AddScoped<IEventSnapshotRepository, EventSnapshotRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOutboxProcessorJob, OutboxProcessorJob>();
        services.AddScoped<IOrderExpirationReconciliationJob, OrderExpirationReconciliationJob>();
        services.AddSingleton<IQrTokenGenerator, QrTokenGenerator>();
        services.AddSingleton<IQrTokenHasher, Sha256QrTokenHasher>();

        return services;
    }

    public static IServiceCollection AddIntegrationEventHandlers(this IServiceCollection services)
    {
        services.AddScoped<OrderExpirationDueMessageHandler>();
        services.AddScoped<PaymentConfirmedIntegrationEventHandler>();
        services.AddScoped<PaymentFailedIntegrationEventHandler>();
        services.AddScoped<EventCreatedEventHandler>();
        services.AddScoped<EventUpdatedEventHandler>();
        services.AddScoped<TicketTypeCreatedEventHandler>();
        services.AddScoped<TicketCapacityAddedEventHandler>();
        services.AddScoped<TicketTypeDeletedEventHandler>();

        return services;
    }
}
