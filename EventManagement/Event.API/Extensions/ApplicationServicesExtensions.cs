using Eventbox.EventManagement.EventApi.Application.Abstractions;
using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;
using Eventbox.EventManagement.EventApi.Infrastructure;
using Eventbox.EventManagement.EventApi.Infrastructure.Repositories;
using Eventbox.EventManagement.EventApi.Services;
using Eventbox.EventManagement.EventApi.Services.Abstractions;
using Eventbox.EventManagement.Infrastructure.Jobs;
using Eventbox.Shared.Outbox;

namespace Eventbox.EventManagement.EventApi.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<ITicketTypeRepository, TicketTypeRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IOrganizationMemberRepository, OrganizationMemberRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrganizerEventService, OrganizerEventService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IPublicEventService, PublicEventService>();
        services.AddScoped<ITicketTypeService, TicketTypeService>();
        services.AddScoped<IOutboxProcessorJob, OutboxProcessorJob>();

        return services;
    }
}
