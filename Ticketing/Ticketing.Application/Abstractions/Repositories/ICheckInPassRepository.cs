using Eventbox.TicketingDomain.Entities.CheckInAggregate;

namespace Eventbox.TicketingApplication.Abstractions.Repositories;

public interface ICheckInPassRepository
{
    Task<CheckInPass?> GetByQrTokenHashAsync(string qrTokenHash, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CheckInPass>> GetByRegistrationOrderIdAsync(Guid registrationOrderId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByQrTokenHashAsync(string qrTokenHash, CancellationToken cancellationToken = default);
    Task<bool> ExistsByRegistrationTicketIdAsync(Guid registrationTicketId, CancellationToken cancellationToken = default);
    Task AddAsync(CheckInPass checkInPass, CancellationToken cancellationToken = default);
    void Update(CheckInPass checkInPass);
}
