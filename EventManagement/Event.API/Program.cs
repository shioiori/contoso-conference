using Eventbox.EventManagement.EventApi.Infrastructure;
using Eventbox.EventManagement.EventApi.Infrastructure.Repositories;
using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;
using Eventbox.EventManagement.EventApi.Mappings;
using Eventbox.EventManagement.EventApi.Services;
using Eventbox.EventManagement.EventApi.Services.Abstractions;
using Eventbox.EventBus.RabbitMQ.Extensions;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var mapsterConfig = TypeAdapterConfig.GlobalSettings;
mapsterConfig.Apply(new EventMappingConfig());

builder.Services.AddDbContext<EventDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddRabbitMqEventBus(builder.Configuration);

builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ISeatTypeRepository, SeatTypeRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IOrganizerEventService, OrganizerEventService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IPublicEventService, PublicEventService>();
builder.Services.AddScoped<ISeatTypeService, SeatTypeService>();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
