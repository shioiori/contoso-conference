namespace Eventbox.Payment.Api.Services;

public interface IOrderPaymentStarter
{
    Task<StartPaymentOutcome> StartPaymentAsync(Guid orderId, CancellationToken cancellationToken);
}

public enum StartPaymentResult
{
    Started,
    OrderNotPayable,
    OrderNotFound,
    ServiceUnavailable
}

public class StartPaymentOutcome
{
    public StartPaymentOutcome(StartPaymentResult result, decimal amount = 0, string currency = "VND")
    {
        Result = result;
        Amount = amount;
        Currency = currency;
    }

    public StartPaymentResult Result { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
