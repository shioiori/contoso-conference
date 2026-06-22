using Eventbox.Payment.Core.Enums;
using Eventbox.Shared.SeedWorks;

namespace Eventbox.Payment.Core.Entities
{
    public class Payment : AuditableEntity<Guid>
    {
        private Payment(
            Guid orderId,
            decimal amount,
            string currency,
            string? returnUrl,
            string? cancelUrl,
            string? idempotencyKey) : base(Guid.NewGuid())
        {
            if (orderId == Guid.Empty)
                throw new ArgumentException("Order id is required.", nameof(orderId));

            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount cannot be negative.");

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency is required.", nameof(currency));

            OrderId = orderId;
            Amount = amount;
            Currency = currency.Trim().ToUpperInvariant();
            ReturnUrl = returnUrl;
            CancelUrl = cancelUrl;
            IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim();
            Status = PaymentStatus.Pending;
        }

        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public DateTimeOffset? FailedAt { get; set; }
        public string? IdempotencyKey { get; set; }
        public string? ProviderEventId { get; set; }
        public string? FailureReason { get; set; }
        public string? ReturnUrl { get; set; }
        public string? CancelUrl { get; set; }

        public static Payment CreateIntent(
            Guid orderId,
            decimal amount,
            string currency,
            string? returnUrl,
            string? cancelUrl,
            string? idempotencyKey)
            => new(orderId, amount, currency, returnUrl, cancelUrl, idempotencyKey);

        public bool MarkSucceeded(
            string providerEventId,
            Guid orderId,
            decimal amount,
            string currency,
            DateTimeOffset paidAt)
        {
            ValidateCallback(providerEventId, orderId, amount, currency);

            if (IsDuplicateCallback(providerEventId, PaymentStatus.Succeeded))
                return false;

            if (Status != PaymentStatus.Pending)
                throw new InvalidOperationException($"Payment intent '{Id}' is already {Status}.");

            CaptureCallbackAmountIfNeeded(amount, currency);
            ProviderEventId = providerEventId;
            Status = PaymentStatus.Succeeded;
            CompletedAt = paidAt;
            FailureReason = null;
            return true;
        }

        public bool MarkFailed(
            string providerEventId,
            Guid orderId,
            decimal amount,
            string currency,
            DateTimeOffset failedAt,
            string? failureReason)
        {
            ValidateCallback(providerEventId, orderId, amount, currency);

            if (IsDuplicateCallback(providerEventId, PaymentStatus.Failed))
                return false;

            if (Status != PaymentStatus.Pending)
                throw new InvalidOperationException($"Payment intent '{Id}' is already {Status}.");

            CaptureCallbackAmountIfNeeded(amount, currency);
            ProviderEventId = providerEventId;
            Status = PaymentStatus.Failed;
            FailedAt = failedAt;
            FailureReason = failureReason;
            return true;
        }

        private bool IsDuplicateCallback(string providerEventId, PaymentStatus expectedStatus)
            => string.Equals(ProviderEventId, providerEventId, StringComparison.Ordinal)
                && Status == expectedStatus;

        private void ValidateCallback(string providerEventId, Guid orderId, decimal amount, string currency)
        {
            if (string.IsNullOrWhiteSpace(providerEventId))
                throw new ArgumentException("Provider event id is required.", nameof(providerEventId));

            if (orderId != OrderId)
                throw new InvalidOperationException("Payment callback order does not match the payment intent.");

            if (amount < 0)
                throw new InvalidOperationException("Payment callback amount cannot be negative.");

            var normalizedCurrency = currency.Trim().ToUpperInvariant();
            if (!string.Equals(Currency, normalizedCurrency, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Payment callback currency does not match the payment intent.");

            if (Amount > 0 && Amount != amount)
                throw new InvalidOperationException("Payment callback amount does not match the payment intent.");
        }

        private void CaptureCallbackAmountIfNeeded(decimal amount, string currency)
        {
            if (Amount == 0 && amount > 0)
            {
                Amount = amount;
                Currency = currency.Trim().ToUpperInvariant();
            }
        }
    }
}
