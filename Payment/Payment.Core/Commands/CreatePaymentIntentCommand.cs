using Eventbox.Payment.Core.Abstractions;
using Eventbox.Payment.Core.Responses;
using Eventbox.Shared.Exceptions;
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
        string? IdempotencyKey) : IRequest<PaymentIntentResponse>;

    public class CreatePaymentIntentCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<CreatePaymentIntentCommand, PaymentIntentResponse>
    {
        public async Task<PaymentIntentResponse> Handle(
            CreatePaymentIntentCommand request,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                var existing = await unitOfWork.Payments.GetByIdempotencyKeyAsync(
                    request.IdempotencyKey,
                    cancellationToken);

                if (existing is not null)
                    return existing.Adapt<PaymentIntentResponse>();
            }

            var payment = PaymentEntity.CreateIntent(
                request.OrderId,
                request.Amount,
                request.Currency,
                request.ReturnUrl,
                request.CancelUrl,
                request.IdempotencyKey);

            try
            {
                await unitOfWork.Payments.AddAsync(payment, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex) when (ex.InnerException?.Message.Contains("IX_Payments_OrderId_Pending") == true)
            {
                throw new ConflictException("A pending payment already exists for this order.");
            }

            return payment.Adapt<PaymentIntentResponse>();
        }
    }
}
