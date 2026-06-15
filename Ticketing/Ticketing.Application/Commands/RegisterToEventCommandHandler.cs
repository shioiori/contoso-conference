using Eventbox.Shared.Exceptions;
using Eventbox.Ticketing.Application.Abstractions;
using Eventbox.Ticketing.Application.Abstractions.Jobs;
using Eventbox.Ticketing.Application.Constants;
using Eventbox.Ticketing.Application.Dtos;
using Eventbox.Ticketing.Domain.Entities.OrderAggregate;
using Mapster;
using MediatR;
using System.Security.Cryptography;
using System.Text;

namespace Eventbox.Ticketing.Application.Commands
{
    public class RegisterToEventCommandHandler(
        IOrderExpirationScheduler orderExpirationScheduler,
        IUnitOfWork unitOfWork) : IRequestHandler<RegisterToEventCommand, OrderDto>
    {
        public async Task<OrderDto> Handle(RegisterToEventCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ValidationApiException("Buyer email is required.");

            if (request.Quantity <= 0)
                throw new ValidationApiException("Quantity must be greater than zero.");

            if (request.Quantity > RegistrationConstants.MaxTicketsPerOrder)
                throw new ValidationApiException($"Cannot reserve more than {RegistrationConstants.MaxTicketsPerOrder} tickets per order.");

            var utcNow = DateTimeOffset.UtcNow;
            var reservationExpiresAt = utcNow.AddMinutes(RegistrationConstants.ReservationExpirationMinutes);

            var ticketAvailability = await unitOfWork.TicketAvailabilities.GetByEventIdAsync(request.EventId, cancellationToken)
                ?? throw new NotFoundException($"Ticket availability for event '{request.EventId}' was not found.");

            var ticketType = ticketAvailability.GetTicketType(request.TicketTypeId);
            var accessCodeHash = string.IsNullOrWhiteSpace(request.AccessCode) ? null : HashAccessCode(request.AccessCode);
            ticketType.EnsureCanAccess(accessCodeHash);
            ticketType.EnsureOrderQuantityAllowed(request.Quantity);
            var pricingPhase = ticketType.GetActivePricingPhase(utcNow);

            var hasActivePendingOrder = await unitOfWork.Orders.HasActivePendingOrderAsync(
                request.EventId,
                request.UserId,
                request.Email,
                utcNow,
                cancellationToken);
            if (hasActivePendingOrder)
                throw new ConflictException("Buyer already has an active pending order for this event.");

            var order = new Order(
                request.EventId,
                request.UserId,
                new PersonalInfo(request.Name, request.Email),
                new[]
                {
                    new OrderItem(
                        ticketType.Id,
                        pricingPhase.Id,
                        ticketType.Name,
                        pricingPhase.Name,
                        pricingPhase.Price,
                        ticketType.Currency,
                        request.Quantity)
                },
                GenerateAccessCode(),
                reservationExpiresAt);

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var reserved = await unitOfWork.TicketAvailabilities.TryReserveAsync(
                    request.EventId,
                    request.TicketTypeId,
                    request.Quantity,
                    cancellationToken);

                if (!reserved)
                    throw new ConflictException("Not enough tickets remaining.");

                await unitOfWork.Orders.AddAsync(order, cancellationToken);
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

        private static string HashAccessCode(string accessCode)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(accessCode.Trim()));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
