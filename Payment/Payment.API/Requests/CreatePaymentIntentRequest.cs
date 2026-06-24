namespace Eventbox.Payment.Api.Requests
{
    public class CreatePaymentIntentRequest
    {
        public Guid OrderId { get; set; }
        public string ReturnUrl { get; set; }
        public string CancelUrl { get; set; }
        public string OrderAccessCode { get; set; }
    }
}
