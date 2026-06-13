namespace Eventbox.Payment.Api.Requests
{
    public class SimulatedPaymentCallbackRequest
    {
        public string ProviderEventId { get; set; } = string.Empty;
        public Guid PaymentIntentId { get; set; }
        public Guid OrderId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTimeOffset? PaidAt { get; set; }
        public DateTimeOffset? FailedAt { get; set; }
        public string? FailureReason { get; set; }
    }
}
