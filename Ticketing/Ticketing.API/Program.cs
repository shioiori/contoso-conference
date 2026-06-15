using Eventbox.EventBus.RabbitMQ.Extensions;
using Eventbox.TicketingApi.Endpoints;
using Eventbox.TicketingApplication.Abstractions;
using Eventbox.TicketingApplication.Abstractions.Jobs;
using Eventbox.TicketingApi.HostedServices;
using Eventbox.TicketingApplication.Commands;
using Eventbox.TicketingApplication.IntegrationEventHandlers;
using Eventbox.TicketingApplication.IntegrationEvents;
using Eventbox.TicketingApplication.Mappings;
using Eventbox.TicketingApplication.MessageHandlers;
using Eventbox.TicketingApplication.Messages;
using Eventbox.TicketingInfrastructure;
using Eventbox.TicketingInfrastructure.Jobs;
using Eventbox.TicketingInfrastructure.Messaging;
using Eventbox.TicketingInfrastructure.Repositories;
using Eventbox.TicketingInfrastructure.Security;
using Hangfire;
using Hangfire.PostgreSql;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Eventbox.TicketingApplication.Abstractions.Repositories;
using Eventbox.Shared.Auditing;
using Eventbox.Shared.Exceptions;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddEventboxSerilog("TicketingAPI");
builder.Services.AddEventboxExceptionHandling();

var mapsterConfig = TypeAdapterConfig.GlobalSettings;
mapsterConfig.Apply(new RegistrationMapping());

builder.Services.AddDbContext<RegistrationDbContext>((serviceProvider, options) =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("Database"))
        .AddInterceptors(
            serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>(),
            serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ITicketAvailabilityRepository, TicketAvailabilityRepository>();
builder.Services.AddScoped<IEventScheduleRepository, EventScheduleRepository>();
builder.Services.AddScoped<IRegistrationUnitOfWork, RegistrationUnitOfWork>();
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
    var db = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
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
    "registration-expire-orders-reconciliation",
    job => job.RunAsync(CancellationToken.None),
    "*/5 * * * *");

app.Run();
