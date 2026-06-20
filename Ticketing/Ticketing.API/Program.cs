using Eventbox.Contracts.IntegrationEvents;
using Eventbox.Ticketing.Infrastructure.Jobs;
using Eventbox.Shared.Auditing;
using Eventbox.Shared.Exceptions;
using Eventbox.Shared.Outbox;
using Eventbox.Ticketing.Api.Endpoints;
using Eventbox.Ticketing.Api.HostedServices;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Jobs;
using Eventbox.Ticketing.Application.Abstractions.Repositories;
using Eventbox.Ticketing.Application.Commands;
using Eventbox.Ticketing.Application.IntegrationEventHandlers;
using Eventbox.Ticketing.Application.Mappings;
using Eventbox.Ticketing.Infrastructure;
using Eventbox.Ticketing.Infrastructure.Repositories;
using Eventbox.Ticketing.Infrastructure.Security;
using Hangfire;
using Hangfire.PostgreSql;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EventBus.RabbitMQ;
using Eventbox.EventBus.Core.Abstractions;

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
builder.Services.AddScoped<IOutboxProcessorJob, OutboxProcessorJob>();
builder.Services.AddSingleton<IQrTokenGenerator, QrTokenGenerator>();
builder.Services.AddSingleton<IQrTokenHasher, Sha256QrTokenHasher>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<RegisterToEventCommand>());
builder.Services.AddEventboxMediatRAuditLogging();

builder.Services.Configure<RabbitMQOptions>(options =>
{
    builder.Configuration.GetSection("RabbitMQ").Bind(options);
    options.Subscribe<PaymentConfirmedIntegrationEvent>("eventbox.payment");
    options.Subscribe<PaymentFailedIntegrationEvent>("eventbox.payment");
    options.Subscribe<EventCreatedEvent>("eventbox.events");
    options.Subscribe<EventUpdatedEvent>("eventbox.events");
    options.Subscribe<TicketTypeCreatedEvent>("eventbox.ticketing");
    options.Subscribe<TicketCapacityAddedEvent>("eventbox.ticketing");
    options.Subscribe<TicketTypeDeletedEvent>("eventbox.ticketing");
    options.Subscribe<OrderExpirationDueMessageIntergrationEvent>(
        "eventbox.ticketing",
        routingKey: "ticketing.expire",
        queue: "eventbox.ticketing.expire");
});

builder.Services.AddSingleton<RabbitMQEventBus>();
builder.Services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<RabbitMQEventBus>());
builder.Services.AddSingleton<IDelayedEventScheduler>(sp => sp.GetRequiredService<RabbitMQEventBus>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<RabbitMQEventBus>());
builder.Services.AddScoped<OrderExpirationDueMessageHandler>();
builder.Services.AddScoped<PaymentConfirmedIntegrationEventHandler>();
builder.Services.AddScoped<PaymentFailedIntegrationEventHandler>();
builder.Services.AddScoped<EventCreatedEventHandler>();
builder.Services.AddScoped<EventUpdatedEventHandler>();
builder.Services.AddScoped<TicketTypeCreatedEventHandler>();
builder.Services.AddScoped<TicketCapacityAddedEventHandler>();
builder.Services.AddScoped<TicketTypeDeletedEventHandler>();

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
    "ticketing-expire-orders-reconciliation",
    job => job.RunAsync(CancellationToken.None),
    Cron.Minutely());

RecurringJob.AddOrUpdate<IOutboxProcessorJob>(
    "ticketing-outbox-processor",
    job => job.RunAsync(CancellationToken.None),
    Cron.Minutely());

app.Run();
