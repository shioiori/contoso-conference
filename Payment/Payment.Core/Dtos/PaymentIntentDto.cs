using Eventbox.Payment.Core.Enums;

namespace Eventbox.Payment.Core.Dtos
{
    public class PaymentIntentDto
    {
        public Guid PaymentIntentId { get; set; }
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public string? ReturnUrl { get; set; }
        public string? CancelUrl { get; set; }
    }

    public class PaymentCallbackResponse
    {
        public Guid PaymentIntentId { get; set; }
        public Guid OrderId { get; set; }
        public PaymentStatus Status { get; set; }
        public string ProviderEventId { get; set; } = string.Empty;
        public bool Processed { get; set; }
    }
}
