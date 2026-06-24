using Eventbox.EventManagement.EventApi.Api.Extensions;
using Eventbox.EventManagement.EventApi.Api.Mappings;
using Eventbox.Shared.Auditing;
using Eventbox.Shared.Exceptions;
using Mapster;

var builder = WebApplication.CreateBuilder(args);

builder.AddEventboxSerilog("Event.API");
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEventboxExceptionHandling();
builder.Services.AddEventboxMediatRAuditLogging();

TypeAdapterConfig.GlobalSettings.Apply(new EventMappingConfig());

builder.Services.AddHttpContextAccessor();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddHangfireWithPostgres(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

app.MigrateDatabase();

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
app.UseRecurringJobs();

app.Run();
