using Eventbox.Payment.Core.Enums;

namespace Eventbox.Payment.Core.Responses
{
    public class PaymentIntentResponse
    {
        public Guid PaymentIntentId { get; set; }
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public PaymentStatus Status { get; set; }
        public string? ReturnUrl { get; set; }
        public string? CancelUrl { get; set; }
    }
}
