using Eventbox.Payment.Core.Enums;

namespace Eventbox.Payment.Core.Entities
{
    public class Payment
    {
        private Payment()
        {
            Currency = null!;
        }

        private Payment(
            Guid orderId,
            decimal amount,
            string currency,
            string? returnUrl,
            string? cancelUrl,
            string? idempotencyKey)
        {
            if (orderId == Guid.Empty)
                throw new ArgumentException("Order id is required.", nameof(orderId));

            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount cannot be negative.");

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency is required.", nameof(currency));

            Id = Guid.NewGuid();
            OrderId = orderId;
            Amount = amount;
            Currency = currency.Trim().ToUpperInvariant();
            ReturnUrl = returnUrl;
            CancelUrl = cancelUrl;
            IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim();
            Status = Enums.PaymentStatus.Pending;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public Guid Id { get; private set; }
        public Guid OrderId { get; private set; }
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }
        public Enums.PaymentStatus Status { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? CompletedAt { get; private set; }
        public DateTimeOffset? FailedAt { get; private set; }
        public string? IdempotencyKey { get; private set; }
        public string? ProviderEventId { get; private set; }
        public string? FailureReason { get; private set; }
        public string? ReturnUrl { get; private set; }
        public string? CancelUrl { get; private set; }

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

            if (IsDuplicateCallback(providerEventId, Enums.PaymentStatus.Succeeded))
                return false;

            if (Status != Enums.PaymentStatus.Pending)
                throw new InvalidOperationException($"Payment intent '{Id}' is already {Status}.");

            CaptureCallbackAmountIfNeeded(amount, currency);
            ProviderEventId = providerEventId;
            Status = Enums.PaymentStatus.Succeeded;
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

            if (IsDuplicateCallback(providerEventId, Enums.PaymentStatus.Failed))
                return false;

            if (Status != Enums.PaymentStatus.Pending)
                throw new InvalidOperationException($"Payment intent '{Id}' is already {Status}.");

            CaptureCallbackAmountIfNeeded(amount, currency);
            ProviderEventId = providerEventId;
            Status = Enums.PaymentStatus.Failed;
            FailedAt = failedAt;
            FailureReason = failureReason;
            return true;
        }

        private bool IsDuplicateCallback(string providerEventId, Enums.PaymentStatus expectedStatus)
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
