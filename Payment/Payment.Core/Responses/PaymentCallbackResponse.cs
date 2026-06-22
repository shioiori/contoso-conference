using Eventbox.Payment.Core.Enums;

namespace Eventbox.Payment.Core.Responses
{
    public class PaymentCallbackResponse
    {
        public Guid PaymentIntentId { get; set; }
        public Guid OrderId { get; set; }
        public PaymentStatus Status { get; set; }
        public string ProviderEventId { get; set; }
        public bool Processed { get; set; }
    }
}
