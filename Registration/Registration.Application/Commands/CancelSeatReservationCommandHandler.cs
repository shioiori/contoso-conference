using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Registration.Application.Commands
{
    public class CancelSeatReservationCommandHandler : IRequestHandler<CancelSeatReservationCommand, bool>
    {
        public Task<bool> Handle(CancelSeatReservationCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
