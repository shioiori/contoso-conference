using MediatR;
using Registration.Application.Commands;
using Registration.Application.Queries;

namespace Registration.API.Apis
{
    public static class OrderApis
    {
        public static RouteGroupBuilder MapOrderApis(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("api/registration");
            app.MapPost("/register", RegisterToConference);
            app.MapPut("/{orderId:guid}/confirm", ConfirmOrderPayment);
            app.MapPut("/{orderId:guid}/cancel", CancelOrder);
            app.MapGet("/{orderId:guid}", GetOrderDetail);
            app.MapGet("/{email}", GetOrdersByEmail);
            return api;
        }

        public static async Task<IResult> RegisterToConference(IMediator mediator, RegisterToConferenceCommand command)
        {
            var result = await mediator.Send(command);
            return Results.Ok();
        }

        public static async Task<IResult> ConfirmOrderPayment(Guid orderId, IMediator mediator)
        {
            var command = new ConfirmOrderPaymentCommand { OrderId = orderId };
            var result = await mediator.Send(command);
            return Results.Ok();
        }
        public static async Task<IResult> CancelOrder(Guid orderId, IMediator mediator)
        {
            var command = new CancelOrderCommand { OrderId = orderId };
            var result = await mediator.Send(command);
            return Results.Ok();
        }

        public static async Task<IResult> GetOrderDetail(Guid orderId, IOrderQueries orderQueries)
        {
            var result = await orderQueries.GetOrderDetail(orderId);
            if (result is null)
            {
                return Results.NotFound();
            }
            return Results.Ok(result);
        }

        public static async Task<IResult> GetOrdersByEmail(string email, IOrderQueries orderQueries)
        {
            var result = await orderQueries.GetOrdersByEmail(email);
            if (result is null || !result.Any())
            {
                return Results.NotFound();
            }
            return Results.Ok(result);

        }
    }
}
