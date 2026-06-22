using MediatR;
using Eventbox.Ticketing.Api.Requests;
using Eventbox.Shared.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Eventbox.Ticketing.Application.Commands.Orders;
using Eventbox.Ticketing.Application.Queries.Inventory;
using Eventbox.Ticketing.Application.Queries.Orders;

namespace Eventbox.Ticketing.Api.Endpoints
{
    public static class OrderEndpoints
    {
        public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
        {
            var publicApi = app.MapGroup("api/public");

            publicApi.MapPost("/events/{eventId:guid}/orders", RegisterToEvent);
            publicApi.MapGet("/self-service/orders", GetOrdersBySelfServiceToken);

            var customerApi = app.MapGroup("api/customer")
                .RequireAuthorization("RequireCustomerAccount");

            customerApi.MapGet("/orders", GetCurrentCustomerOrders);
            customerApi.MapGet("/orders/by-email", GetOrdersByEmail);
            customerApi.MapGet("/orders/{orderId:guid}", GetOrderDetail);
            customerApi.MapPost("/orders/{orderId:guid}/confirm-free", ConfirmFreeOrder);
            customerApi.MapPost("/orders/{orderId:guid}/cancel", CancelOrder);

            return app;
        }

        public static async Task<IResult> RegisterToEvent(Guid eventId, HttpContext httpContext, IMediator mediator, RegisterToEventRequest request, CancellationToken cancellationToken)
        {
            var customerIdentity = GetCustomerIdentity(httpContext.User);
            var email = customerIdentity.Email ?? request.Email;
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ValidationApiException("Email is required for guest checkout.");
            }

            var command = new RegisterToEventCommand
            {
                EventId = eventId,
                UserId = customerIdentity.UserId,
                Name = request.Name,
                Email = email,
                TicketTypeId = request.TicketTypeId,
                Quantity = request.Quantity,
                AccessCode = request.AccessCode
            };

            var result = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/public/orders/{result.Id}", result);
        }

        public static async Task<IResult> ConfirmFreeOrder(Guid orderId, HttpContext httpContext, IMediator mediator, CancellationToken cancellationToken)
        {
            var accessResult = await EnsureCurrentCustomerCanAccessOrder(orderId, httpContext, mediator, cancellationToken);
            if (accessResult is not null)
            {
                return accessResult;
            }

            var command = new ConfirmOrderCommand { OrderId = orderId };
            await mediator.Send(command, cancellationToken);
            return Results.NoContent();
        }

        public static async Task<IResult> CancelOrder(Guid orderId, HttpContext httpContext, IMediator mediator, CancellationToken cancellationToken)
        {
            var accessResult = await EnsureCurrentCustomerCanAccessOrder(orderId, httpContext, mediator, cancellationToken);
            if (accessResult is not null)
            {
                return accessResult;
            }

            var command = new CancelOrderCommand { OrderId = orderId };
            await mediator.Send(command, cancellationToken);
            return Results.NoContent();
        }

        public static async Task<IResult> GetOrderDetail(Guid orderId, HttpContext httpContext, IMediator mediator, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetOrderDetailQuery(orderId), cancellationToken);
            if (result is null)
            {
                throw new NotFoundException("Order", orderId);
            }

            if (!CurrentCustomerCanAccessOrder(httpContext, result.UserId, result.Email))
            {
                return Results.Forbid();
            }

            return Results.Ok(result);
        }

        public static async Task<IResult> GetOrdersByEmail(string email, HttpContext httpContext, IMediator mediator, CancellationToken cancellationToken)
        {
            var customerIdentity = GetCustomerIdentity(httpContext.User);
            if (!string.Equals(customerIdentity.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                return Results.Forbid();
            }

            var result = await mediator.Send(new GetOrdersByEmailQuery(email), cancellationToken);
            return Results.Ok(result);

        }

        public static async Task<IResult> GetOrdersBySelfServiceToken(string token, IMediator mediator, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetOrderBySelfServiceTokenQuery(token), cancellationToken);
            if (result is null)
                throw new NotFoundException("Order lookup token was not found.");

            return Results.Ok(result);
        }

        public static async Task<IResult> GetCurrentCustomerOrders(HttpContext httpContext, IMediator mediator, CancellationToken cancellationToken)
        {
            var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value
                ?? httpContext.User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ValidationApiException("Customer token does not include an email claim.");
            }

            var result = await mediator.Send(new GetOrdersByEmailQuery(email), cancellationToken);
            return Results.Ok(result);
        }

        private static async Task<IResult?> EnsureCurrentCustomerCanAccessOrder(
            Guid orderId,
            HttpContext httpContext,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            var order = await mediator.Send(new GetOrderDetailQuery(orderId), cancellationToken);
            if (order is null)
            {
                throw new NotFoundException("Order", orderId);
            }

            return CurrentCustomerCanAccessOrder(httpContext, order.UserId, order.Email)
                ? null
                : Results.Forbid();
        }

        private static bool CurrentCustomerCanAccessOrder(HttpContext httpContext, Guid? orderUserId, string orderEmail)
        {
            var customerIdentity = GetCustomerIdentity(httpContext.User);
            if (customerIdentity.UserId.HasValue && orderUserId == customerIdentity.UserId)
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(customerIdentity.Email)
                && string.Equals(customerIdentity.Email, orderEmail, StringComparison.OrdinalIgnoreCase);
        }

        private static (Guid? UserId, string? Email) GetCustomerIdentity(ClaimsPrincipal user)
        {
            if (user.Identity?.IsAuthenticated != true)
            {
                return (null, null);
            }

            var accountType = user.FindFirst("account_type")?.Value;
            if (!string.Equals(accountType, "Customer", StringComparison.Ordinal))
            {
                return (null, null);
            }

            var idValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var email = user.FindFirst(ClaimTypes.Email)?.Value
                ?? user.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

            return (Guid.TryParse(idValue, out var userId) ? userId : null, email);
        }
    }
}
