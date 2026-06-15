using Eventbox.EventBus.RabbitMQ.Extensions;
using Eventbox.Ticketing.Api.Endpoints;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Jobs;
using Eventbox.Ticketing.Api.HostedServices;
using Eventbox.Ticketing.Application.Commands;
using Eventbox.Ticketing.Application.IntegrationEventHandlers;
using Eventbox.Ticketing.Application.IntegrationEvents;
using Eventbox.Ticketing.Application.MessageHandlers;
using Eventbox.Ticketing.Application.Messages;
using Eventbox.Ticketing.Infrastructure;
using Eventbox.Ticketing.Infrastructure.Jobs;
using Eventbox.Ticketing.Infrastructure.Messaging;
using Eventbox.Ticketing.Infrastructure.Repositories;
using Hangfire;
using Hangfire.PostgreSql;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using System.Text;
using Eventbox.Shared.Auditing;
using Eventbox.Ticketing.Infrastructure.Security;
using Eventbox.Shared.Exceptions;
using Eventbox.Ticketing.Application.Mappings;

var builder = WebApplication.CreateBuilder(args);

builder.AddEventboxSerilog("Ticketing.Api");
builder.Services.AddEventboxExceptionHandling();

var mapsterConfig = TypeAdapterConfig.GlobalSettings;
mapsterConfig.Apply(new TicketingMapping());

builder.Services.AddDbContext<TicketingDbContext>((serviceProvider, options) =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("Database"))
        .AddInterceptors(
            serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>(),
            serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ITicketAvailabilityRepository, TicketAvailabilityRepository>();
builder.Services.AddScoped<IEventScheduleRepository, EventScheduleRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderExpirationScheduler, OrderExpirationScheduler>();
builder.Services.AddSingleton<IQrTokenGenerator, QrTokenGenerator>();
builder.Services.AddSingleton<IQrTokenHasher, Sha256QrTokenHasher>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<RegisterToEventCommand>());
builder.Services.AddEventboxMediatRAuditLogging();

builder.Services.AddRabbitMqEventBus(builder.Configuration);
builder.Services.AddIntegrationEventHandler<
    OrderExpirationDueMessage,
    OrderExpirationDueMessageHandler>();
builder.Services.AddIntegrationEventHandler<
    PaymentConfirmedIntegrationEvent,
    PaymentConfirmedIntegrationEventHandler>();
builder.Services.AddIntegrationEventHandler<
    EventCreatedEvent,
    EventCreatedEventHandler>();
builder.Services.AddIntegrationEventHandler<
    EventUpdatedEvent,
    EventUpdatedEventHandler>();
builder.Services.AddIntegrationEventHandler<
    TicketTypeCreatedEvent,
    TicketTypeCreatedEventHandler>();
builder.Services.AddIntegrationEventHandler<
    TicketCapacityAddedEvent,
    TicketCapacityAddedEventHandler>();
builder.Services.AddHostedService<RabbitMqSubscriptionHostedService>();

builder.Services.AddHangfire(config =>
{
    var connectionString = builder.Configuration.GetConnectionString("Database");
    config.UsePostgreSqlStorage(options =>
    {
        options.UseNpgsqlConnection(connectionString);
    });
});
builder.Services.AddHangfireServer();

builder.Services.AddScoped<IOrderExpirationReconciliationJob, OrderExpirationReconciliationJob>();

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Eventbox.Auth";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "Eventbox.Api";
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"] ?? "eventbox-development-signing-key-change-me";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireCustomerAccount", policy =>
        policy.RequireAuthenticatedUser()
            .RequireClaim("account_type", "Customer"));
    options.AddPolicy("RequireOrganizerAccount", policy =>
        policy.RequireAuthenticatedUser()
            .RequireClaim("account_type", "Organizer"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();
    db.Database.Migrate();
}

app.UseEventboxExceptionHandling();
app.UseAuthentication();
app.UseEventboxSerilogRequestLogging();
app.UseAuthorization();

app.MapOrderEndpoints();
app.MapTicketAvailabilityEndpoints();
app.MapCheckInEndpoints();

app.UseHangfireDashboard("/hangfire");

RecurringJob.AddOrUpdate<IOrderExpirationReconciliationJob>(
    "Ticketing-expire-orders-reconciliation",
    job => job.RunAsync(CancellationToken.None),
    "*/5 * * * *");

app.Run();
