using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eventbox.TicketingApplication.Commands
{
    public class CancelOrderCommand : IRequest<bool>
    {
        public Guid OrderId { get; set; }
    }
}
