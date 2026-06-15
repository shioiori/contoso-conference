using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Eventbox.TicketingApi.Requests;
using Eventbox.TicketingApplication.Commands;
using Eventbox.TicketingDomain.Enums;
using MediatR;

namespace Eventbox.TicketingApi.Endpoints;

public static class CheckInEndpoints
{
    public static IEndpointRouteBuilder MapCheckInEndpoints(this IEndpointRouteBuilder app)
    {
        var organizerApi = app.MapGroup("api/events/{eventId:guid}/check-ins")
            .RequireAuthorization("RequireOrganizerAccount");

        organizerApi.MapPost("/", CheckInQr);

        return app;
    }

    private static async Task<IResult> CheckInQr(
        Guid eventId,
        CheckInQrRequest request,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CheckInByQrTokenCommand
        {
            EventId = eventId,
            QrToken = request.QrToken,
            StaffUserId = GetCurrentUserId(httpContext.User)
        }, cancellationToken);

        return ToHttpResult(result.Result, result);
    }

    private static IResult ToHttpResult(CheckInAttemptResult result, object body)
        => result switch
        {
            CheckInAttemptResult.Success => Results.Ok(body),
            CheckInAttemptResult.InvalidToken => Results.NotFound(body),
            CheckInAttemptResult.WrongEvent => Results.BadRequest(body),
            CheckInAttemptResult.Cancelled => Results.BadRequest(body),
            CheckInAttemptResult.CheckInUnavailable => Results.BadRequest(body),
            CheckInAttemptResult.AlreadyCheckedIn => Results.Conflict(body),
            _ => Results.BadRequest(body)
        };

    private static Guid? GetCurrentUserId(ClaimsPrincipal user)
    {
        var idValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(idValue, out var userId) ? userId : null;
    }
}
