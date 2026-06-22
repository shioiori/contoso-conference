using Eventbox.Payment.Api.Enums;
using Eventbox.Payment.Api.Options;
using Eventbox.Payment.Api.Requests;
using Eventbox.Payment.Api.Services;
using Eventbox.Payment.Core.Commands;
using Eventbox.Shared.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Eventbox.Payment.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentController(
        IMediator mediator,
        IOptions<PaymentOptions> paymentOptions,
        IOrderAccessVerifier orderAccessVerifier) : ControllerBase
    {
        [HttpPost("intents")]
        public async Task<IActionResult> CreatePaymentIntent(
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
            [FromBody] CreatePaymentIntentRequest request,
            CancellationToken cancellationToken)
        {
            var accessResult = await orderAccessVerifier.VerifyAsync(
                request.OrderId,
                request.OrderAccessCode,
                Request.Headers.Authorization.ToString(),
                cancellationToken);

            EnsureAccess(accessResult);

            var result = await mediator.Send(
                new CreatePaymentIntentCommand(
                    request.OrderId,
                    request.Amount,
                    request.Currency ?? "USD",
                    request.ReturnUrl,
                    request.CancelUrl,
                    idempotencyKey),
                cancellationToken);

            return Ok(result);
        }

        [HttpPost("simulated-callbacks")]
        public async Task<IActionResult> SimulateCallbacks(
            [FromHeader(Name = "X-Provider-Signature")] string? providerSignature,
            [FromBody] SimulatedPaymentCallbackRequest request,
            CancellationToken cancellationToken)
        {
            var expectedSignature = paymentOptions.Value.ProviderSignature;
            if (string.IsNullOrWhiteSpace(expectedSignature) || providerSignature != expectedSignature)
            {
                throw new UnauthorizedApiException("Invalid payment provider signature.");
            }

            object result;
            if (string.Equals(request.Status, "Succeeded", StringComparison.OrdinalIgnoreCase))
            {
                result = await mediator.Send(
                    new SimulatePaymentSucceededCommand(
                        request.PaymentIntentId,
                        request.ProviderEventId,
                        request.OrderId,
                        request.Amount,
                        request.Currency,
                        request.PaidAt ?? DateTimeOffset.UtcNow),
                    cancellationToken);
            }
            else if (string.Equals(request.Status, "Failed", StringComparison.OrdinalIgnoreCase))
            {
                result = await mediator.Send(
                    new SimulatePaymentFailedCommand(
                        request.PaymentIntentId,
                        request.ProviderEventId,
                        request.OrderId,
                        request.Amount,
                        request.Currency,
                        request.FailedAt ?? DateTimeOffset.UtcNow,
                        request.FailureReason),
                    cancellationToken);
            }
            else
            {
                throw new ValidationApiException("Unsupported payment callback status.");
            }

            return Ok(result);
        }

        private static void EnsureAccess(OrderAccessVerificationResult accessResult)
        {
            switch (accessResult)
            {
                case OrderAccessVerificationResult.Authorized:
                    return;
                case OrderAccessVerificationResult.Unauthorized:
                    throw new UnauthorizedApiException("Authentication or order access code is required.");
                case OrderAccessVerificationResult.Forbidden:
                    throw new ForbiddenApiException("Access to this order is forbidden.");
                case OrderAccessVerificationResult.NotFound:
                    throw new NotFoundException("Order was not found.");
                case OrderAccessVerificationResult.RegistrationUnavailable:
                    throw new ServiceUnavailableException("Registration service is unavailable.");
                default:
                    throw new ServiceUnavailableException("Order access could not be verified.");
            }
        }
    }
}
