using MediatR;

namespace Registration.Application.Commands
{
    public class MakeSeatReservationCommand : IRequest<bool>
    {
        public Guid OrderId { get; init; }
    }
}