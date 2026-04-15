using MediatR;

namespace Registration.Application.Commands
{
    public record RegisterToConferenceCommandHandler() : IRequestHandler<RegisterToConferenceCommand, bool>
    {
        public Task<bool> Handle(RegisterToConferenceCommand request, CancellationToken cancellationToken)
        {
            
            throw new NotImplementedException();
        }
    }
}