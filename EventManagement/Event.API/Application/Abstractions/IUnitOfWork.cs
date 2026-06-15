using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;

namespace Eventbox.EventManagement.EventApi.Application.Abstractions
{
    public interface IUnitOfWork
    {
        IEventRepository Events { get; }
        ISeatTypeRepository SeatTypes { get; }
        IOrganizationRepository Organizations { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
