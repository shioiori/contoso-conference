using Eventbox.Registration.Application.Queries;
using MediatR;

namespace Eventbox.Registration.Api.Endpoints
{
    public static class SeatAvailabilityEndpoints
    {
        public static RouteGroupBuilder MapSeatAvailabilityEndpoints(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("api/public/events");
            api.MapGet("/{eventId:guid}/seat-availability", GetSeatAvailability);
            return api;
        }

        public static async Task<IResult> GetSeatAvailability(Guid eventId, IMediator mediator, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetSeatAvailabilityQuery(eventId), cancellationToken);
            if (result is null)
            {
                return Results.NotFound();
            }
            return Results.Ok(result);
        }
    }
}
