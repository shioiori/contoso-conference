using MediatR;

namespace Registration.Application.Commands
{
    public class ConfirmOrderPaymentCommandHandler : IRequestHandler<ConfirmOrderPaymentCommand, bool>
    {
        public Task<bool> Handle(ConfirmOrderPaymentCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}