using Eventbox.Ticketing.Application.Queries.Inventory;
using MediatR;

namespace Eventbox.Ticketing.Api.Endpoints
{
    public static class TicketAvailabilityEndpoints
    {
        public static RouteGroupBuilder MapTicketAvailabilityEndpoints(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("api/public/tickets");
            api.MapGet("/{eventId:guid}/availability", GetTicketAvailability);
            return api;
        }

        public static async Task<IResult> GetTicketAvailability(Guid eventId, IMediator mediator, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetTicketAvailabilityQuery(eventId), cancellationToken);
            return Results.Ok(result);
        }
    }
}
