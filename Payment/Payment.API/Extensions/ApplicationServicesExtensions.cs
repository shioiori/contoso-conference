using Eventbox.Payment.Core.Abstractions;
using Eventbox.Payment.Core.Commands;
using Eventbox.Payment.Infrastructure.Jobs;
using Eventbox.Payment.Infrastructure.Persistence;
using Eventbox.Shared.Outbox;
using Eventbox.Payment.Api.Options;
using Eventbox.Payment.Api.Services;
using Microsoft.Extensions.Options;

namespace Eventbox.Payment.Api.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PaymentOptions>()
            .Bind(configuration.GetSection(PaymentOptions.SectionName));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreatePaymentIntentCommand>());
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOutboxProcessorJob, OutboxProcessorJob>();

        var ticketingApiOptions = configuration
            .GetSection(TicketingOptions.SectionName)
            .Get<TicketingOptions>() ?? new TicketingOptions();

        if (string.IsNullOrWhiteSpace(ticketingApiOptions.BaseUrl))
            throw new InvalidOperationException("TicketingApi:BaseUrl must be configured.");

        services.AddHttpClient<TicketingOrderAccessVerifier>(client =>
        {
            client.BaseAddress = new Uri(ticketingApiOptions.BaseUrl);
        });
        services.AddScoped<IOrderAccessVerifier>(sp => sp.GetRequiredService<TicketingOrderAccessVerifier>());
        services.AddScoped<IOrderPaymentStarter>(sp => sp.GetRequiredService<TicketingOrderAccessVerifier>());

        return services;
    }
}
