using Eventbox.Payment.Core.Abstractions;
using Eventbox.Payment.Core.Dtos;
using Eventbox.Shared.Exceptions;
using MediatR;

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

    public class SimulatePaymentFailedCommandHandler(IPaymentRepository paymentRepository)
        : IRequestHandler<SimulatePaymentFailedCommand, PaymentCallbackResponse>
    {
        public async Task<PaymentCallbackResponse> Handle(
            SimulatePaymentFailedCommand request,
            CancellationToken cancellationToken)
        {
            var duplicate = await paymentRepository.GetByProviderEventIdAsync(
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

            var payment = await paymentRepository.GetByIdAsync(request.PaymentIntentId, cancellationToken)
                ?? throw new NotFoundException("Payment intent", request.PaymentIntentId);

            var processed = payment.MarkFailed(
                request.ProviderEventId,
                request.OrderId,
                request.Amount,
                request.Currency,
                request.FailedAt,
                request.FailureReason);

            await paymentRepository.SaveChangesAsync(cancellationToken);

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
