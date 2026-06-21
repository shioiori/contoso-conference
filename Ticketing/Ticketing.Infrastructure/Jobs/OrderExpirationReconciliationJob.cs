using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Jobs;
using Eventbox.Ticketing.Domain.Enums;

namespace Eventbox.Ticketing.Infrastructure.Jobs
{
    public class OrderExpirationReconciliationJob(
        IUnitOfWork unitOfWork) : IOrderExpirationReconciliationJob
    {
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var utcNow = DateTimeOffset.UtcNow;
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
                    var ticketAvailability = await unitOfWork.TicketAvailabilities.GetByEventIdAsync(order.EventId, cancellationToken);
                    if (ticketAvailability is not null)
                    {
                        foreach (var item in order.OrderItems)
                        {
                            ticketAvailability.Release(item.TicketTypeId, item.Quantity);
                        }

                        }

                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
