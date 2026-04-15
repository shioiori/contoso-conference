using MediatR;

namespace Registration.Application.Commands
{
    public class ConfirmOrderPaymentCommand : IRequest<bool>
    {
        public Guid OrderId { get; init; }
    }
}