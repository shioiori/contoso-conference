using Eventbox.Registration.Application.Abstractions;
using Eventbox.Registration.Application.Abstractions.Jobs;
using Eventbox.Registration.Domain.Enums;
using Eventbox.Registration.Application.Abstractions.Repositories;

namespace Eventbox.Registration.Infrastructure.Jobs
{
    public class OrderExpirationReconciliationJob(
        IOrderRepository orderRepository,
        ISeatAvailabilityRepository seatAvailabilityRepository,
        IRegistrationUnitOfWork unitOfWork) : IOrderExpirationReconciliationJob
    {
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var utcNow = DateTimeOffset.UtcNow;
            var expiredOrders = orderRepository
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
                    var seatAvailability = await seatAvailabilityRepository.GetByEventIdAsync(order.EventId, cancellationToken);
                    if (seatAvailability is not null)
                    {
                        foreach (var item in order.OrderItems)
                        {
                            seatAvailability.Release(item.TicketTypeId, item.Quantity);
                        }

                        seatAvailabilityRepository.Update(seatAvailability);
                    }

                    orderRepository.Update(order);
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
