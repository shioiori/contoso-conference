using Eventbox.Payment.Api.Enums;
using Eventbox.Payment.Api.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Eventbox.Payment.Api.Services;

public class TicketingOrderAccessVerifier(
    HttpClient httpClient,
    IOptions<TicketingOptions> ticketingOptions) : IOrderAccessVerifier, IOrderPaymentStarter
{
    public const string InternalServiceTokenHeaderName = "X-Internal-Service-Token";

    public async Task<OrderAccessVerificationResult> VerifyAsync(
        Guid orderId,
        string orderAccessCode,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        OrderAccessVerificationResult? guestResult = null;
        if (!string.IsNullOrWhiteSpace(orderAccessCode))
        {
            guestResult = await VerifyGuestAccessAsync(orderId, orderAccessCode, cancellationToken);
            if (guestResult == OrderAccessVerificationResult.Authorized)
            {
                return guestResult.Value;
            }
        }

        if (!string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return await VerifyCustomerAccessAsync(orderId, authorizationHeader, cancellationToken);
        }

        return guestResult ?? OrderAccessVerificationResult.Unauthorized;
    }

    private async Task<OrderAccessVerificationResult> VerifyGuestAccessAsync(
        Guid orderId,
        string orderAccessCode,
        CancellationToken cancellationToken)
    {
        var requestUri = $"/api/public/self-service/orders?token={Uri.EscapeDataString(orderAccessCode)}";
        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync(requestUri, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return OrderAccessVerificationResult.RegistrationUnavailable;
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return OrderAccessVerificationResult.Forbidden;
            }

            if (!response.IsSuccessStatusCode)
            {
                return OrderAccessVerificationResult.RegistrationUnavailable;
            }

            TicketingOrderDto? order;
            try
            {
                order = await response.Content.ReadFromJsonAsync<TicketingOrderDto>(cancellationToken);
            }
            catch (JsonException)
            {
                return OrderAccessVerificationResult.RegistrationUnavailable;
            }

            return order?.Id == orderId
                ? OrderAccessVerificationResult.Authorized
                : OrderAccessVerificationResult.Forbidden;
        }
    }

    private async Task<OrderAccessVerificationResult> VerifyCustomerAccessAsync(
        Guid orderId,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/customer/orders/{orderId}");
        try
        {
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(authorizationHeader);
        }
        catch (FormatException)
        {
            return OrderAccessVerificationResult.Unauthorized;
        }

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return OrderAccessVerificationResult.RegistrationUnavailable;
        }

        using (response)
        {
            return response.StatusCode switch
            {
                HttpStatusCode.OK => OrderAccessVerificationResult.Authorized,
                HttpStatusCode.Unauthorized => OrderAccessVerificationResult.Unauthorized,
                HttpStatusCode.Forbidden => OrderAccessVerificationResult.Forbidden,
                HttpStatusCode.NotFound => OrderAccessVerificationResult.NotFound,
                _ => OrderAccessVerificationResult.RegistrationUnavailable
            };
        }
    }

    public async Task<StartPaymentOutcome> StartPaymentAsync(Guid orderId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/internal/orders/{orderId}/start-payment");
        request.Headers.Add(InternalServiceTokenHeaderName, ticketingOptions.Value.InternalServiceToken);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return new StartPaymentOutcome(StartPaymentResult.ServiceUnavailable);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
                return new StartPaymentOutcome(StartPaymentResult.OrderNotFound);

            if (response.StatusCode == HttpStatusCode.Conflict)
                return new StartPaymentOutcome(StartPaymentResult.OrderNotPayable);

            if (!response.IsSuccessStatusCode)
                return new StartPaymentOutcome(StartPaymentResult.ServiceUnavailable);

            TicketingOrderAmountDto? amount;
            try
            {
                amount = await response.Content.ReadFromJsonAsync<TicketingOrderAmountDto>(cancellationToken);
            }
            catch (JsonException)
            {
                return new StartPaymentOutcome(StartPaymentResult.ServiceUnavailable);
            }

            return amount is not null
                ? new StartPaymentOutcome(StartPaymentResult.Started, amount.TotalAmount, amount.Currency)
                : new StartPaymentOutcome(StartPaymentResult.ServiceUnavailable);
        }
    }

    private sealed class TicketingOrderDto
    {
        public Guid Id { get; init; }
    }

    private sealed class TicketingOrderAmountDto
    {
        public decimal TotalAmount { get; init; }
        public string? Currency { get; init; }
    }
}
