using Eventbox.Shared.Outbox;
using Hangfire;
using Hangfire.PostgreSql;

namespace Eventbox.EventManagement.EventApi.Api.Extensions;

public static class HangfireExtensions
{
    public static IServiceCollection AddHangfireWithPostgres(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(config =>
        {
            var connectionString = configuration.GetConnectionString("Database");
            config.UsePostgreSqlStorage(options =>
            {
                options.UseNpgsqlConnection(connectionString);
            });
        });
        services.AddHangfireServer();

        return services;
    }

    public static IApplicationBuilder UseRecurringJobs(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

        recurringJobManager.AddOrUpdate<IOutboxProcessorJob>(
            "event-outbox-processor",
            job => job.RunAsync(CancellationToken.None),
            "*/30 * * * * *");

        return app;
    }
}
