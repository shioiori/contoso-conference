using Eventbox.Ticketing.Application.Queries;
using MediatR;

namespace Eventbox.Ticketing.Api.Endpoints
{
    public static class TicketAvailabilityEndpoints
    {
        public static RouteGroupBuilder MapTicketAvailabilityEndpoints(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("api/public/events");
            api.MapGet("/{eventId:guid}/ticket-availability", GetTicketAvailability);
            return api;
        }

        public static async Task<IResult> GetTicketAvailability(Guid eventId, IMediator mediator, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetTicketAvailabilityQuery(eventId), cancellationToken);
            if (result is null)
            {
                return Results.NotFound();
            }
            return Results.Ok(result);
        }
    }
}
