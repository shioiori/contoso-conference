using Eventbox.Payment.Core.Abstractions;
using Eventbox.Payment.Core.Dtos;
using MediatR;

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
        IPaymentRepository paymentRepository,
        IPaymentEventPublisher paymentEventPublisher)
        : IRequestHandler<SimulatePaymentSucceededCommand, PaymentCallbackResponse>
    {
        public async Task<PaymentCallbackResponse> Handle(
            SimulatePaymentSucceededCommand request,
            CancellationToken cancellationToken)
        {
            var duplicate = await paymentRepository.GetByProviderEventIdAsync(
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

            var payment = await paymentRepository.GetByIdAsync(request.PaymentIntentId, cancellationToken)
                ?? throw new KeyNotFoundException($"Payment intent '{request.PaymentIntentId}' was not found.");

            var processed = payment.MarkSucceeded(
                request.ProviderEventId,
                request.OrderId,
                request.Amount,
                request.Currency,
                request.PaidAt);

            await paymentRepository.SaveChangesAsync(cancellationToken);

            if (processed)
            {
                await paymentEventPublisher.PublishPaymentConfirmedAsync(
                    payment,
                    request.ProviderEventId,
                    request.PaidAt,
                    cancellationToken);
            }

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
