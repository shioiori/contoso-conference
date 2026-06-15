using Eventbox.TicketingApplication.Abstractions.Repositories;
using Eventbox.TicketingDomain.Entities.CheckInAggregate;
using Microsoft.EntityFrameworkCore;

namespace Eventbox.TicketingInfrastructure.Repositories;

public class CheckInPassRepository(RegistrationDbContext dbContext) : ICheckInPassRepository
{
    public Task<CheckInPass?> GetByQrTokenHashAsync(string qrTokenHash, CancellationToken cancellationToken = default)
        => dbContext.CheckInPasses.FirstOrDefaultAsync(p => p.QrTokenHash == qrTokenHash, cancellationToken);

    public async Task<IReadOnlyCollection<CheckInPass>> GetByRegistrationOrderIdAsync(Guid registrationOrderId, CancellationToken cancellationToken = default)
        => await dbContext.CheckInPasses
            .Where(p => p.RegistrationOrderId == registrationOrderId)
            .OrderBy(p => p.TicketTypeId)
            .ThenBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsByQrTokenHashAsync(string qrTokenHash, CancellationToken cancellationToken = default)
        => dbContext.CheckInPasses.AnyAsync(p => p.QrTokenHash == qrTokenHash, cancellationToken);

    public Task<bool> ExistsByRegistrationTicketIdAsync(Guid registrationTicketId, CancellationToken cancellationToken = default)
        => dbContext.CheckInPasses.AnyAsync(p => p.RegistrationTicketId == registrationTicketId, cancellationToken);

    public async Task AddAsync(CheckInPass checkInPass, CancellationToken cancellationToken = default)
        => await dbContext.CheckInPasses.AddAsync(checkInPass, cancellationToken);

    public void Update(CheckInPass checkInPass)
        => dbContext.CheckInPasses.Update(checkInPass);
}
