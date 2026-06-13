using Eventbox.EventManagement.EventApi.Domains;

namespace Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories
{
    public interface IOrganizationRepository : IRepository<Organization, Guid>
    {
    }
}
