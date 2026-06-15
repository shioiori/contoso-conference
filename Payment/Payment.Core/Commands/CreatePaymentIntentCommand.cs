using Eventbox.Payment.Core.Abstractions;
using Eventbox.Payment.Core.Dtos;
using Mapster;
using MediatR;
using PaymentEntity = Eventbox.Payment.Core.Entities.Payment;

namespace Eventbox.Payment.Core.Commands
{
    public record CreatePaymentIntentCommand(
        Guid OrderId,
        decimal Amount,
        string Currency,
        string? ReturnUrl,
        string? CancelUrl,
        string? IdempotencyKey) : IRequest<PaymentIntentDto>;

    public class CreatePaymentIntentCommandHandler(IPaymentRepository paymentRepository)
        : IRequestHandler<CreatePaymentIntentCommand, PaymentIntentDto>
    {
        public async Task<PaymentIntentDto> Handle(
            CreatePaymentIntentCommand request,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                var existing = await paymentRepository.GetByIdempotencyKeyAsync(
                    request.IdempotencyKey,
                    cancellationToken);

                if (existing is not null)
                    return existing.Adapt<PaymentIntentDto>();
            }

            var payment = PaymentEntity.CreateIntent(
                request.OrderId,
                request.Amount,
                request.Currency,
                request.ReturnUrl,
                request.CancelUrl,
                request.IdempotencyKey);

            await paymentRepository.AddAsync(payment, cancellationToken);
            await paymentRepository.SaveChangesAsync(cancellationToken);

            return payment.Adapt<PaymentIntentDto>();
        }
    }
}
