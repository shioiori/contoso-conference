namespace Eventbox.Payment.Api.Services;

public interface IOrderAccessVerifier
{
    Task<OrderAccessVerificationResult> VerifyAsync(
        Guid orderId,
        string? orderAccessCode,
        string? authorizationHeader,
        CancellationToken cancellationToken);
}
