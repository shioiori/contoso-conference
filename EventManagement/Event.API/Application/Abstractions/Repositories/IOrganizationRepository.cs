using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.Shared.Abstractions;

namespace Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories
{
    public interface IOrganizationRepository : IRepository<Organization, Guid>;
}
