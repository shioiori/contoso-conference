using Eventbox.Shared.Auditing;
using Eventbox.Ticketing.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Eventbox.Ticketing.Api.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TicketingDbContext>((serviceProvider, options) =>
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
        var db = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();
        db.Database.Migrate();

        return app;
    }
}
