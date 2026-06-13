using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;
using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.EventManagement.EventApi.Infrastructure.Repositories.Common;

namespace Eventbox.EventManagement.EventApi.Infrastructure.Repositories
{
    public class OrganizationRepository(EventDbContext dbContext) : BaseRepository<EventDbContext, Organization, Guid>(dbContext), IOrganizationRepository
    {
    }
}
