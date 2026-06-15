using Eventbox.EventBus.RabbitMQ.Extensions;
using Eventbox.Registration.Api.Endpoints;
using Eventbox.Registration.Application.Abstractions;
using Eventbox.Registration.Application.Abstractions.Jobs;
using Eventbox.Registration.Api.HostedServices;
using Eventbox.Registration.Application.Commands;
using Eventbox.Registration.Application.IntegrationEventHandlers;
using Eventbox.Registration.Application.IntegrationEvents;
using Eventbox.Registration.Application.Mappings;
using Eventbox.Registration.Application.MessageHandlers;
using Eventbox.Registration.Application.Messages;
using Eventbox.Registration.Infrastructure;
using Eventbox.Registration.Infrastructure.Jobs;
using Eventbox.Registration.Infrastructure.Messaging;
using Hangfire;
using Hangfire.PostgreSql;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var mapsterConfig = TypeAdapterConfig.GlobalSettings;
mapsterConfig.Apply(new RegistrationMapping());

builder.Services.AddDbContext<RegistrationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderExpirationScheduler, OrderExpirationScheduler>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<RegisterToEventCommand>());

builder.Services.AddRabbitMqEventBus(builder.Configuration);
builder.Services.AddIntegrationEventHandler<
    OrderExpirationDueMessage,
    OrderExpirationDueMessageHandler>();
builder.Services.AddIntegrationEventHandler<
    PaymentConfirmedIntegrationEvent,
    PaymentConfirmedIntegrationEventHandler>();
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
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapOrderEndpoints();
app.MapSeatAvailabilityEndpoints();

app.UseHangfireDashboard("/hangfire");

RecurringJob.AddOrUpdate<IOrderExpirationReconciliationJob>(
    "registration-expire-orders-reconciliation",
    job => job.RunAsync(CancellationToken.None),
    "*/5 * * * *");

app.Run();
