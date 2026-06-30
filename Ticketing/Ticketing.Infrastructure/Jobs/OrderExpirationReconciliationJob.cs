using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Jobs;
using Eventbox.Ticketing.Domain.Enums;

namespace Eventbox.Ticketing.Infrastructure.Jobs
{
    public class OrderExpirationReconciliationJob(
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider) : IOrderExpirationReconciliationJob
    {
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var utcNow = timeProvider.GetUtcNow();
            var expiredOrders = unitOfWork.Orders
                .Get(
                    x => x.OrderState == OrderState.Pending
                        && x.ReservationExpiresAt <= utcNow,
                    includeProperties: "OrderItems",
                    needAsNoTracking: false)
                .ToList();

            foreach (var order in expiredOrders)
            {
                var stateChanged = order.Expire(utcNow);
                if (stateChanged)
                {
                    var ticketTypes = await unitOfWork.TicketTypeAvailabilities.GetByEventIdAsync(order.EventId, cancellationToken);
                    foreach (var item in order.OrderItems)
                        ticketTypes.FirstOrDefault(t => t.Id == item.TicketTypeId)?.Release(item.Quantity);

                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
