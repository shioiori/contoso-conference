using Eventbox.EventBus.RabbitMQ.Extensions;
using Eventbox.Payment.Api.Options;
using Eventbox.Payment.Api.Services;
using Eventbox.Payment.Core.Abstractions;
using Eventbox.Payment.Core.Commands;
using Eventbox.Payment.Core.Mappings;
using Eventbox.Payment.Infrastructure.Messaging;
using Eventbox.Payment.Infrastructure.Persistence;
using Eventbox.Shared.Auditing;
using Eventbox.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddEventboxSerilog("Payment.API");
builder.Services.AddControllers();
builder.Services.AddEventboxExceptionHandling();
TypeAdapterConfig.GlobalSettings.Apply(new PaymentMapping());
var ticketingApiOptions = builder.Configuration
    .GetSection(TicketingApiOptions.SectionName)
    .Get<TicketingApiOptions>() ?? new TicketingApiOptions();
if (string.IsNullOrWhiteSpace(ticketingApiOptions.BaseUrl))
{
    throw new InvalidOperationException("TicketingApi:BaseUrl must be configured.");
}

builder.Services.AddDbContext<PaymentDbContext>((serviceProvider, options) =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("Database"))
        .AddInterceptors(
            serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>(),
            serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreatePaymentIntentCommand>());
builder.Services.AddEventboxMediatRAuditLogging();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<UnitOfWork>();
builder.Services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<UnitOfWork>());
builder.Services.AddScoped<IOutbox>(serviceProvider => serviceProvider.GetRequiredService<UnitOfWork>());
builder.Services.AddScoped<IPaymentEventPublisher, RabbitMqPaymentEventPublisher>();
builder.Services.AddHttpClient<IOrderAccessVerifier, TicketingOrderAccessVerifier>(client =>
{
    client.BaseAddress = new Uri(ticketingApiOptions.BaseUrl);
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
