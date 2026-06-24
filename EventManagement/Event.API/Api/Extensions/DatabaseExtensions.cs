using Eventbox.EventManagement.EventApi.Infrastructure;
using Eventbox.Shared.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Eventbox.EventManagement.EventApi.Api.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EventDbContext>((serviceProvider, options) =>
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
        var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        db.Database.Migrate();

        return app;
    }
}
