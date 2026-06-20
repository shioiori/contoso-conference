using Eventbox.Payment.Api.Extensions;
using Eventbox.Payment.Core.Mappings;
using Eventbox.Shared.Auditing;
using Eventbox.Shared.Exceptions;
using Mapster;

var builder = WebApplication.CreateBuilder(args);

builder.AddEventboxSerilog("Payment.API");
builder.Services.AddControllers();
builder.Services.AddEventboxExceptionHandling();
builder.Services.AddEventboxMediatRAuditLogging();

TypeAdapterConfig.GlobalSettings.Apply(new PaymentMapping());

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddHangfireWithPostgres(builder.Configuration);

var app = builder.Build();

app.MigrateDatabase();

app.UseEventboxExceptionHandling();
app.UseEventboxSerilogRequestLogging();
app.MapControllers();
app.UseRecurringJobs();

app.Run();
