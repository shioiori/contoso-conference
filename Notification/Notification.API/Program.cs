using Eventbox.Notification.Api.Extensions;
using Eventbox.Notification.Api.HostedServices;
using Eventbox.Shared.Auditing;
using Eventbox.Shared.Exceptions;

var builder = WebApplication.CreateBuilder(args);

builder.AddEventboxSerilog("Notification.API");
builder.Services.AddEventboxExceptionHandling();

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddHostedService<RabbitMqSubscriptionHostedService>();

var app = builder.Build();

app.UseEventboxExceptionHandling();
app.UseEventboxSerilogRequestLogging();

app.Run();
