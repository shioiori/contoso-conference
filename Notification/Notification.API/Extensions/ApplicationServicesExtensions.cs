using Eventbox.Notification.Api.Email;
using Eventbox.Notification.Api.IntegrationEvents.EventHandlers;

namespace Eventbox.Notification.Api.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailOptions>(configuration.GetSection("Smtp"));
        services.AddTransient<IEmailSender, SmtpEmailSender>();
        services.AddTransient<OrderConfirmedIntegrationEventHandler>();
        return services;
    }
}
