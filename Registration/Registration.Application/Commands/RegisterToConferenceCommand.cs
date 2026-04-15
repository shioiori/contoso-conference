using MediatR;
using Registration.Domain.Entities.OrderAggregate;
using System.Windows.Input;

namespace Registration.Application.Commands
{
    public record RegisterToConferenceCommand : IRequest<bool>
    {
        public Guid ConferenceId { get; init; }
        public PersonalInfo PersonalInfo { get; init; }
        public int SeatTypeId { get; init; }
    }
}