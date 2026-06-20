using Eventbox.Shared.Outbox;
using Eventbox.Ticketing.Application.Abstractions.Jobs;
using Hangfire;
using Hangfire.PostgreSql;

namespace Eventbox.Ticketing.Api.Extensions;

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
        app.UseHangfireDashboard("/hangfire");

        using var scope = app.ApplicationServices.CreateScope();
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

        recurringJobManager.AddOrUpdate<IOrderExpirationReconciliationJob>(
            "ticketing-expire-orders-reconciliation",
            job => job.RunAsync(CancellationToken.None),
            Cron.Minutely());

        recurringJobManager.AddOrUpdate<IOutboxProcessorJob>(
            "ticketing-outbox-processor",
            job => job.RunAsync(CancellationToken.None),
            Cron.Minutely());

        return app;
    }
}
