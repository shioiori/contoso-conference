using MediatR;
using System.Security.Cryptography;
using Eventbox.Registration.Application.Abstractions;
using Eventbox.Registration.Application.Abstractions.Jobs;
using Eventbox.Registration.Application.Constants;
using Eventbox.Registration.Application.Dtos;
using Eventbox.Registration.Domain.Entities.OrderAggregate;
using Eventbox.Registration.Application.Abstractions.Repositories;
using Mapster;

namespace Eventbox.Registration.Application.Commands
{
    public class RegisterToEventCommandHandler(
        IOrderRepository orderRepository,
        ISeatAvailabilityRepository seatAvailabilityRepository,
        IOrderExpirationScheduler orderExpirationScheduler,
        IRegistrationUnitOfWork unitOfWork) : IRequestHandler<RegisterToEventCommand, OrderDto>
    {
        public async Task<OrderDto> Handle(RegisterToEventCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new InvalidOperationException("Buyer email is required.");

            if (request.Quantity <= 0)
                throw new InvalidOperationException("Quantity must be greater than zero.");

            if (request.Quantity > RegistrationConstants.MaxTicketsPerOrder)
                throw new InvalidOperationException($"Cannot reserve more than {RegistrationConstants.MaxTicketsPerOrder} tickets per order.");

            var utcNow = DateTimeOffset.UtcNow;
            var reservationExpiresAt = request.ReservationExpiresAt ?? utcNow.AddMinutes(RegistrationConstants.ReservationExpirationMinutes);
            if (reservationExpiresAt <= utcNow)
                throw new InvalidOperationException("Reservation expiration must be in the future.");

            var hasActivePendingOrder = await orderRepository.HasActivePendingOrderAsync(
                request.EventId,
                request.UserId,
                request.Email,
                utcNow,
                cancellationToken);
            if (hasActivePendingOrder)
                throw new InvalidOperationException("Buyer already has an active pending order for this event.");

            var order = new Order(
                request.EventId,
                request.UserId,
                new PersonalInfo(request.Name, request.Email),
                new[] { new OrderItem(request.TicketTypeId, request.Quantity) },
                GenerateAccessCode(),
                reservationExpiresAt);

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var reserved = await seatAvailabilityRepository.TryReserveAsync(
                    request.EventId,
                    request.TicketTypeId,
                    request.Quantity,
                    cancellationToken);

                if (!reserved)
                    throw new InvalidOperationException("Not enough seats remaining.");

                await orderRepository.AddAsync(order, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);

            await orderExpirationScheduler.ScheduleExpirationAsync(
                order.Id,
                order.ReservationExpiresAt!.Value,
                cancellationToken);

            return order.Adapt<OrderDto>();
        }

        private static string GenerateAccessCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Range(0, 8)
                .Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)])
                .ToArray());
        }
    }
}
