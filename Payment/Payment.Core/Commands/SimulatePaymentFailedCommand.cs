using Eventbox.Contracts.IntegrationEvents;
using Eventbox.Payment.Core.Abstractions;
using Eventbox.Payment.Core.Responses;
using Eventbox.Shared.Exceptions;
using Eventbox.Shared.Outbox;
using MediatR;
using System.Text.Json;

namespace Eventbox.Payment.Core.Commands
{
    public record SimulatePaymentFailedCommand(
        Guid PaymentIntentId,
        string ProviderEventId,
        Guid OrderId,
        decimal Amount,
        string Currency,
        DateTimeOffset FailedAt,
        string? FailureReason) : IRequest<PaymentCallbackResponse>;

    public class SimulatePaymentFailedCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<SimulatePaymentFailedCommand, PaymentCallbackResponse>
    {
        public async Task<PaymentCallbackResponse> Handle(
            SimulatePaymentFailedCommand request,
            CancellationToken cancellationToken)
        {
            var duplicate = await unitOfWork.Payments.GetByProviderEventIdAsync(
                request.ProviderEventId,
                cancellationToken);

            if (duplicate is not null)
            {
                return new PaymentCallbackResponse {
                    PaymentIntentId = duplicate.Id,
                    OrderId = duplicate.OrderId,
                    Status = duplicate.Status,
                    ProviderEventId = request.ProviderEventId,
                    Processed = false
                };
            }

            var payment = await unitOfWork.Payments.GetByIdAsync(request.PaymentIntentId, cancellationToken)
                ?? throw new NotFoundException("Payment intent", request.PaymentIntentId);

            var processed = payment.MarkFailed(
                request.ProviderEventId,
                request.OrderId,
                request.Amount,
                request.Currency,
                request.FailedAt,
                request.FailureReason);

            if (processed)
            {
                await unitOfWork.Outbox.AddAsync(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    IntegrationEventType = nameof(PaymentFailedIntegrationEvent),
                    Content = JsonSerializer.Serialize(new PaymentFailedIntegrationEvent
                    {
                        PaymentId = payment.Id,
                        OrderId = payment.OrderId,
                        FailureReason = request.FailureReason,
                    }),
                    OccurredOnUtc = DateTime.UtcNow,
                    Status = ProcessStatus.Pending
                }, cancellationToken);
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
