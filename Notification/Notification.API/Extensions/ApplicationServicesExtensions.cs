using Eventbox.Notification.Api.Email;
using Eventbox.Notification.Api.IntegrationEvents.EventHandlers;
using Eventbox.Notification.Api.Resources;

namespace Eventbox.Notification.Api.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailOptions>(configuration.GetSection("Smtp"));
        services.AddTransient<IEmailSender, SmtpEmailSender>();
        services.AddLocalization();
        services.AddTransient(typeof(IResourceLocalizer<>), typeof(ResourceLocalizer<>));
        services.AddTransient<OrderConfirmedIntegrationEventHandler>();
        return services;
    }
}
