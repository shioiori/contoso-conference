using Eventbox.TicketingApplication.Dtos;
using Eventbox.TicketingDomain.Entities.CheckInAggregate;

namespace Eventbox.TicketingApplication.Mappings;

public static class CheckInPassMapping
{
    public static CheckInPassDto ToDto(this CheckInPass pass)
        => new(
            pass.Id,
            pass.EventId,
            pass.RegistrationOrderId,
            pass.RegistrationTicketId,
            pass.TicketTypeId,
            pass.AttendeeName,
            pass.AttendeeEmail,
            pass.QrToken,
            pass.Status,
            pass.CheckedInAt);
}
