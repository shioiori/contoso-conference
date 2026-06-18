using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;
using Eventbox.Shared.Outbox;

namespace Eventbox.EventManagement.EventApi.Application.Abstractions
{
    public interface IUnitOfWork
    {
        IEventRepository Events { get; }
        ITicketTypeRepository TicketTypes { get; }
        IOrganizationRepository Organizations { get; }
        IOutbox Outbox { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
