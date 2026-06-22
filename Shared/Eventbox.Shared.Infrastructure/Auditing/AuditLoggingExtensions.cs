using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Eventbox.Shared.Auditing;

public static class AuditLoggingExtensions
{
    public static WebApplicationBuilder AddEventboxSerilog(this WebApplicationBuilder builder, string serviceName)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Service", serviceName);
        });

        builder.Services.AddEventboxAuditLogging();
        return builder;
    }

    public static IServiceCollection AddEventboxAuditLogging(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAuditContextAccessor, HttpAuditContextAccessor>();
        services.AddTransient<SerilogPropertiesMiddleware>();
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddScoped<SaveChangesInterceptor>(provider =>
            provider.GetRequiredService<AuditSaveChangesInterceptor>());

        return services;
    }

    public static IServiceCollection AddEventboxMediatRAuditLogging(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditLoggingBehavior<,>));
        return services;
    }

    public static WebApplication UseEventboxSerilogRequestLogging(this WebApplication app)
    {
        app.UseMiddleware<SerilogPropertiesMiddleware>();
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("OrganizationId", httpContext.Request.RouteValues.TryGetValue("organizationId", out var organizationId)
                    ? organizationId?.ToString() ?? string.Empty
                    : string.Empty);
                diagnosticContext.Set("UserId", httpContext.User.Identity?.IsAuthenticated == true
                    ? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty
                    : string.Empty);
                diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
            };
        });

        return app;
    }
}
