using Eventbox.Ticketing.Api.Extensions;
using Eventbox.Ticketing.Api.Endpoints;
using Eventbox.Ticketing.Api.HostedServices;
using Eventbox.Ticketing.Api.Options;
using Eventbox.Ticketing.Application.Mappings;
using Eventbox.Shared.Auditing;
using Eventbox.Shared.Exceptions;
using Mapster;

var builder = WebApplication.CreateBuilder(args);

builder.AddEventboxSerilog("Ticketing.Api");
builder.Services.AddEventboxExceptionHandling();
builder.Services.AddEventboxMediatRAuditLogging();
builder.Services.Configure<InternalApiOptions>(
    builder.Configuration.GetSection(InternalApiOptions.SectionName));

TypeAdapterConfig.GlobalSettings.Apply(new TicketingMapping());

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddIntegrationEventHandlers();
builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddHangfireWithPostgres(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddHostedService<IntegrationEventSubscriptionHostedService>();

var app = builder.Build();

app.MigrateDatabase();

app.UseEventboxExceptionHandling();
app.UseAuthentication();
app.UseEventboxSerilogRequestLogging();
app.UseAuthorization();

app.MapOrderEndpoints();
app.MapTicketAvailabilityEndpoints();
app.MapCheckInEndpoints();

app.UseRecurringJobs();

app.Run();
