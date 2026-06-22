using Eventbox.Contracts.IntegrationEvents;
using Eventbox.Payment.Core.Abstractions;
using Eventbox.Payment.Core.Responses;
using Eventbox.Shared.Exceptions;
using Eventbox.Shared.Outbox;
using MediatR;
using System.Text.Json;

namespace Eventbox.Payment.Core.Commands
{
    public record SimulatePaymentSucceededCommand(
        Guid PaymentIntentId,
        string ProviderEventId,
        Guid OrderId,
        decimal Amount,
        string Currency,
        DateTimeOffset PaidAt) : IRequest<PaymentCallbackResponse>;

    public class SimulatePaymentSucceededCommandHandler(
        IUnitOfWork unitOfWork)
        : IRequestHandler<SimulatePaymentSucceededCommand, PaymentCallbackResponse>
    {
        public async Task<PaymentCallbackResponse> Handle(
            SimulatePaymentSucceededCommand request,
            CancellationToken cancellationToken)
        {
            var duplicate = await unitOfWork.Payments.GetByProviderEventIdAsync(
                request.ProviderEventId,
                cancellationToken);

            if (duplicate is not null)
            {
                return new PaymentCallbackResponse
                {
                    PaymentIntentId = duplicate.Id,
                    OrderId = duplicate.OrderId,
                    Status = duplicate.Status,
                    ProviderEventId = request.ProviderEventId,
                    Processed = false
                };
            }

            var payment = await unitOfWork.Payments.GetByIdAsync(request.PaymentIntentId, cancellationToken)
                ?? throw new NotFoundException("Payment intent", request.PaymentIntentId);

            var processed = payment.MarkSucceeded(
                request.ProviderEventId,
                request.OrderId,
                request.Amount,
                request.Currency,
                request.PaidAt);

            if (processed)
            {
                var outboxMessage = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    IntegrationEventType = nameof(PaymentConfirmedIntegrationEvent),
                    Content = JsonSerializer.Serialize(new PaymentConfirmedIntegrationEvent
                    {
                        PaymentId = payment.Id,
                        ProviderEventId = request.ProviderEventId,
                        OrderId = payment.OrderId,
                        Amount = payment.Amount,
                        Currency = payment.Currency,
                        PaidAt = request.PaidAt
                    }),
                    OccurredOnUtc = DateTime.UtcNow,
                    Status = ProcessStatus.Pending
                };

                await unitOfWork.Outbox.AddAsync(outboxMessage, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new PaymentCallbackResponse
            {
                PaymentIntentId = payment.Id,
                OrderId = payment.OrderId,
                Status = payment.Status,
                ProviderEventId = request.ProviderEventId,
                Processed = processed
            };
        }
    }
}
