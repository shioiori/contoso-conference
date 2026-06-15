using Eventbox.Registration.Application.Abstractions;
using Eventbox.Registration.Application.Abstractions.Jobs;
using Eventbox.Registration.Domain.Enums;

namespace Eventbox.Registration.Infrastructure.Jobs
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
                    var seatAvailability = await unitOfWork.SeatAvailabilities.GetByEventIdAsync(order.EventId, cancellationToken);
                    if (seatAvailability is not null)
                    {
                        foreach (var item in order.OrderItems)
                        {
                            seatAvailability.Release(item.TicketTypeId, item.Quantity);
                        }

                        unitOfWork.SeatAvailabilities.Update(seatAvailability);
                    }

                    unitOfWork.Orders.Update(order);
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
