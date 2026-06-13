using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eventbox.Registration.Application.Commands
{
    public class CancelOrderCommand : IRequest<bool>
    {
        public Guid OrderId { get; set; }
    }
}
