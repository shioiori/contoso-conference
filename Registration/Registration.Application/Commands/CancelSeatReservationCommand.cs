using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Registration.Application.Commands
{
    public class CancelSeatReservationCommand : IRequest<bool>
    {
    }
}
