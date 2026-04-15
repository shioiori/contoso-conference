using Registration.Application.Queries;

namespace Registration.API.Apis
{
    public static class SeatAvailabilityApis
    {
        public static RouteGroupBuilder MapSeatAvailabilityApis(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("api/registration");
            app.MapPost("/{conferenceId:guid}/seats", GetSeatAvailability);
            return api;
        }

        public static async Task<IResult> GetSeatAvailability(Guid conferenceId, ISeatAvailabilityQueries seatAvailabilityQueries)
        {
            var result = await seatAvailabilityQueries.GetSeatAvailability(conferenceId);
            if (result is null)
            {
                return Results.NotFound();
            }
            return Results.Ok(result);
        }
    }
}
