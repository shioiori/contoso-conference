using Eventbox.Payment.Core.Abstractions;
using Eventbox.Payment.Core.Commands;
using Eventbox.Payment.Infrastructure.Jobs;
using Eventbox.Payment.Infrastructure.Messaging;
using Eventbox.Payment.Infrastructure.Persistence;
using Eventbox.Shared.Outbox;
using Eventbox.Payment.Api.Options;
using Eventbox.Payment.Api.Services;

namespace Eventbox.Payment.Api.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreatePaymentIntentCommand>());
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPaymentEventPublisher, RabbitMqPaymentEventPublisher>();
        services.AddScoped<IOutboxProcessorJob, OutboxProcessorJob>();

        var ticketingApiOptions = configuration
            .GetSection(TicketingApiOptions.SectionName)
            .Get<TicketingApiOptions>() ?? new TicketingApiOptions();

        if (string.IsNullOrWhiteSpace(ticketingApiOptions.BaseUrl))
            throw new InvalidOperationException("TicketingApi:BaseUrl must be configured.");

        services.AddHttpClient<IOrderAccessVerifier, TicketingOrderAccessVerifier>(client =>
        {
            client.BaseAddress = new Uri(ticketingApiOptions.BaseUrl);
        });

        return services;
    }
}
