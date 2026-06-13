using Eventbox.EventBus.RabbitMQ.Extensions;
using Eventbox.Payment.Api.Options;
using Eventbox.Payment.Api.Services;
using Eventbox.Payment.Core.Abstractions;
using Eventbox.Payment.Core.Commands;
using Eventbox.Payment.Infrastructure.Messaging;
using Eventbox.Payment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
var registrationApiOptions = builder.Configuration
    .GetSection(RegistrationApiOptions.SectionName)
    .Get<RegistrationApiOptions>() ?? new RegistrationApiOptions();
if (string.IsNullOrWhiteSpace(registrationApiOptions.BaseUrl))
{
    throw new InvalidOperationException("RegistrationApi:BaseUrl must be configured.");
}

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreatePaymentIntentCommand>());
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentEventPublisher, RabbitMqPaymentEventPublisher>();
builder.Services.AddHttpClient<IOrderAccessVerifier, RegistrationOrderAccessVerifier>(client =>
{
    client.BaseAddress = new Uri(registrationApiOptions.BaseUrl);
});
builder.Services.AddRabbitMqEventBus(builder.Configuration);

var app = builder.Build();

app.MapControllers();

app.Run();
