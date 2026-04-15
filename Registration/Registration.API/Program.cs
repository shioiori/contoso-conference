using Microsoft.EntityFrameworkCore;
using Registration.API.Apis;
using Registration.Domain.Repositories;
using Registration.Infrastructure;
using Registration.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RegistrationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ISeatAvailabilityRepository, SeatAvailabilityRepository>();

var app = builder.Build();

app.MapOrderApis().MapSeatAvailabilityApis();

app.Run();
