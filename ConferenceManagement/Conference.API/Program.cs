using Conference.API.Infrastructure;
using Conference.API.Mappings;
using Conference.API.Repositories;
using Conference.API.Repositories.Abstractions;
using Conference.API.Services;
using Conference.API.Services.Abstractions;
using Contoso.ServiceBus.Extensions;
using Mapster;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var mapsterConfig = TypeAdapterConfig.GlobalSettings;
mapsterConfig.Apply(new ConferenceMappingConfig());

builder.Services.AddDbContext<ConferenceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddRabbitMqServiceBus(builder.Configuration);

builder.Services.AddScoped<IConferenceRepository, ConferenceRepository>();
builder.Services.AddScoped<ISeatTypeRepository, SeatTypeRepository>();
builder.Services.AddScoped<IConferenceService, ConferenceService>();
builder.Services.AddScoped<ISeatTypeService, SeatTypeService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ConferenceDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
