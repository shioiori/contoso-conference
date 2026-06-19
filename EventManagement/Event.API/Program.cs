using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using Eventbox.EventManagement.EventApi.Application.Abstractions;
using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;
using Eventbox.EventManagement.EventApi.Infrastructure;
using Eventbox.EventManagement.EventApi.Infrastructure.Repositories;
using Eventbox.EventManagement.EventApi.Mappings;
using Eventbox.EventManagement.EventApi.Services;
using Eventbox.EventManagement.EventApi.Services.Abstractions;
using Eventbox.EventManagement.Infrastructure.Jobs;
using Eventbox.Shared.Auditing;
using Eventbox.Shared.Exceptions;
using Eventbox.Shared.Outbox;
using EventBus.RabbitMQ;
using Hangfire;
using Hangfire.PostgreSql;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddEventboxSerilog("Event.API");
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEventboxExceptionHandling();

var mapsterConfig = TypeAdapterConfig.GlobalSettings;
mapsterConfig.Apply(new EventMappingConfig());

builder.Services.AddDbContext<EventDbContext>((serviceProvider, options) =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("Database"))
        .AddInterceptors(
            serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>(),
            serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()));

builder.Services.Configure<RabbitMQOptions>(options =>
{
    options.Publish<EventCreatedEvent>("eventbox.events");
    options.Publish<EventUpdatedEvent>("eventbox.events");
    options.Publish<EventPublishedEvent>("eventbox.events");
    options.Publish<EventUnpublishedEvent>("eventbox.events");
    options.Publish<TicketTypeCreatedEvent>("eventbox.ticketing");
    options.Publish<TicketCapacityAddedEvent>("eventbox.ticketing");
    options.Publish<TicketTypeDeletedEvent>("eventbox.ticketing");
});

builder.Services.AddSingleton<RabbitMQEventBus>();
builder.Services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<RabbitMQEventBus>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<RabbitMQEventBus>());

builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ITicketTypeRepository, TicketTypeRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrganizerEventService, OrganizerEventService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IPublicEventService, PublicEventService>();
builder.Services.AddScoped<ITicketTypeService, TicketTypeService>();
builder.Services.AddScoped<IOutboxProcessorJob, OutboxProcessorJob>();

builder.Services.AddHangfire(config =>
{
    var connectionString = builder.Configuration.GetConnectionString("Database");
    config.UsePostgreSqlStorage(options =>
    {
        options.UseNpgsqlConnection(connectionString);
    });
});
builder.Services.AddHangfireServer();

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
    options.AddPolicy("RequireOrganizerAccount", policy =>
        policy.RequireAuthenticatedUser()
            .RequireClaim("account_type", "Organizer"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseEventboxExceptionHandling();
app.UseAuthentication();
app.UseEventboxSerilogRequestLogging();
app.UseAuthorization();
app.MapControllers();

RecurringJob.AddOrUpdate<IOutboxProcessorJob>(
    "Payment-outbox-processor",
    job => job.RunAsync(CancellationToken.None),
    "*/30 * * * * *");

app.Run();
