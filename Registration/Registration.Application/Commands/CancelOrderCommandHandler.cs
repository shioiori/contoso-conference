using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Registration.Application.Commands
{
    public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, bool>
    {
        public Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
