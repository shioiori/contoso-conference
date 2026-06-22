using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;
using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.Shared.Infrastructure.Repositories;

namespace Eventbox.EventManagement.EventApi.Infrastructure.Repositories
{
    public class OrganizationRepository(EventDbContext dbContext)
        : BaseRepository<EventDbContext, Organization, Guid>(dbContext), IOrganizationRepository;
}
