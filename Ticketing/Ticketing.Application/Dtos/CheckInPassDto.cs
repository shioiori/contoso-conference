using Eventbox.TicketingDomain.Enums;

namespace Eventbox.TicketingApplication.Dtos;

public record CheckInPassDto(
    Guid Id,
    Guid EventId,
    Guid RegistrationOrderId,
    Guid RegistrationTicketId,
    int TicketTypeId,
    string AttendeeName,
    string AttendeeEmail,
    string QrToken,
    CheckInPassStatus Status,
    DateTimeOffset? CheckedInAt);
