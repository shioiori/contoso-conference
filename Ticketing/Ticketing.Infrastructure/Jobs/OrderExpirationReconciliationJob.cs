using Eventbox.TicketingApplication.Abstractions;
using Eventbox.TicketingApplication.Abstractions.Jobs;
using Eventbox.TicketingDomain.Enums;
using Eventbox.TicketingApplication.Abstractions.Repositories;

namespace Eventbox.TicketingInfrastructure.Jobs
{
    public class OrderExpirationReconciliationJob(
        IOrderRepository orderRepository,
        ITicketAvailabilityRepository ticketAvailabilityRepository,
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
                    var ticketAvailability = await ticketAvailabilityRepository.GetByEventIdAsync(order.EventId, cancellationToken);
                    if (ticketAvailability is not null)
                    {
                        foreach (var item in order.OrderItems)
                        {
                            ticketAvailability.Release(item.TicketTypeId, item.Quantity);
                        }

                        ticketAvailabilityRepository.Update(ticketAvailability);
                    }

                    orderRepository.Update(order);
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
