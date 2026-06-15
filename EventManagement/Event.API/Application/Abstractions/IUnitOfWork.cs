using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;

namespace Eventbox.EventManagement.EventApi.Application.Abstractions
{
    public interface IUnitOfWork
    {
        IEventRepository Events { get; }
        ITicketTypeRepository TicketTypes { get; }
        IOrganizationRepository Organizations { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
