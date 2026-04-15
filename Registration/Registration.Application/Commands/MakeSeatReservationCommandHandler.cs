using MediatR;

namespace Registration.Application.Commands
{
    public class MakeSeatReservationCommandHandler : IRequestHandler<MakeSeatReservationCommand, bool>
    {
        public Task<bool> Handle(MakeSeatReservationCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}