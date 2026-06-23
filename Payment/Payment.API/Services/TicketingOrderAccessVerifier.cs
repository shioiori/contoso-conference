using Eventbox.Payment.Api.Enums;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Eventbox.Payment.Api.Services;

public class TicketingOrderAccessVerifier(HttpClient httpClient) : IOrderAccessVerifier, IOrderPaymentStarter
{
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

    public async Task<StartPaymentResult> StartPaymentAsync(Guid orderId, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsync(
                $"/api/public/orders/{orderId}/start-payment",
                content: null,
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            return StartPaymentResult.ServiceUnavailable;
        }

        using (response)
        {
            return response.StatusCode switch
            {
                HttpStatusCode.NoContent => StartPaymentResult.Started,
                HttpStatusCode.NotFound => StartPaymentResult.OrderNotFound,
                HttpStatusCode.Conflict => StartPaymentResult.OrderNotPayable,
                _ => StartPaymentResult.ServiceUnavailable
            };
        }
    }

    private sealed class TicketingOrderDto
    {
        public Guid Id { get; init; }
    }
}
