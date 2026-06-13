using Eventbox.Payment.Api.Requests;
using Eventbox.Payment.Api.Services;
using Eventbox.Payment.Core.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Eventbox.Payment.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentController(
        IMediator mediator,
        IConfiguration configuration,
        IOrderAccessVerifier orderAccessVerifier) : ControllerBase
    {
        [HttpPost("intents")]
        public async Task<IActionResult> CreatePaymentIntent(
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
            [FromBody] CreatePaymentIntentRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var accessResult = await orderAccessVerifier.VerifyAsync(
                    request.OrderId,
                    request.OrderAccessCode,
                    Request.Headers.Authorization.ToString(),
                    cancellationToken);

                var accessError = ToAccessError(accessResult);
                if (accessError is not null)
                {
                    return accessError;
                }

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
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("simulated-callbacks")]
        public async Task<IActionResult> SimulateCallbacks(
            [FromHeader(Name = "X-Provider-Signature")] string? providerSignature,
            [FromBody] SimulatedPaymentCallbackRequest request,
            CancellationToken cancellationToken)
        {
            var expectedSignature = configuration["Payment:ProviderSignature"];
            if (string.IsNullOrWhiteSpace(expectedSignature) || providerSignature != expectedSignature)
            {
                return Unauthorized(new { error = "Invalid payment provider signature." });
            }

            try
            {
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
                    return BadRequest(new { error = "Unsupported payment callback status." });
                }

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private IActionResult? ToAccessError(OrderAccessVerificationResult accessResult)
            => accessResult switch
            {
                OrderAccessVerificationResult.Authorized => null,
                OrderAccessVerificationResult.Unauthorized => Unauthorized(new { error = "Authentication or order access code is required." }),
                OrderAccessVerificationResult.Forbidden => Forbid(),
                OrderAccessVerificationResult.NotFound => NotFound(new { error = "Order was not found." }),
                OrderAccessVerificationResult.RegistrationUnavailable => StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "Registration service is unavailable." }),
                _ => StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "Order access could not be verified." })
            };
    }
}
