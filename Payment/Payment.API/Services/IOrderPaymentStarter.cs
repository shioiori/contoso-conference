namespace Eventbox.Payment.Api.Services;

public interface IOrderPaymentStarter
{
    Task<StartPaymentResult> StartPaymentAsync(Guid orderId, CancellationToken cancellationToken);
}

public enum StartPaymentResult
{
    Started,
    OrderNotPayable,
    OrderNotFound,
    ServiceUnavailable
}
