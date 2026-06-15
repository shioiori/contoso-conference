using Eventbox.EventBus.RabbitMQ.Extensions;
using Eventbox.Payment.Api.Options;
using Eventbox.Payment.Api.Services;
using Eventbox.Payment.Core.Abstractions;
using Eventbox.Payment.Core.Commands;
using Eventbox.Payment.Infrastructure.Messaging;
using Eventbox.Payment.Infrastructure.Persistence;
using Eventbox.Shared.Auditing;
using Eventbox.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddEventboxSerilog("Payment.API");
builder.Services.AddControllers();
builder.Services.AddEventboxExceptionHandling();
var registrationApiOptions = builder.Configuration
    .GetSection(RegistrationApiOptions.SectionName)
    .Get<RegistrationApiOptions>() ?? new RegistrationApiOptions();
if (string.IsNullOrWhiteSpace(registrationApiOptions.BaseUrl))
{
    throw new InvalidOperationException("RegistrationApi:BaseUrl must be configured.");
}

builder.Services.AddDbContext<PaymentDbContext>((serviceProvider, options) =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("Database"))
        .AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreatePaymentIntentCommand>());
builder.Services.AddEventboxMediatRAuditLogging();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentEventPublisher, RabbitMqPaymentEventPublisher>();
builder.Services.AddHttpClient<IOrderAccessVerifier, RegistrationOrderAccessVerifier>(client =>
{
    client.BaseAddress = new Uri(registrationApiOptions.BaseUrl);
});
builder.Services.AddRabbitMqEventBus(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    db.Database.Migrate();
}

app.UseEventboxExceptionHandling();
app.UseEventboxSerilogRequestLogging();
app.MapControllers();

app.Run();
