using Eventbox.Payment.Infrastructure.Persistence;
using Eventbox.Shared.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Eventbox.Payment.Api.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PaymentDbContext>((serviceProvider, options) =>
            options
                .UseNpgsql(configuration.GetConnectionString("Database"))
                .AddInterceptors(
                    serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>(),
                    serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()));

        return services;
    }

    public static IApplicationBuilder MigrateDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        db.Database.Migrate();

        return app;
    }
}
