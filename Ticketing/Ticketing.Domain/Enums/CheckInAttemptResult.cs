namespace Eventbox.Ticketing.Domain.Enums;

public enum CheckInAttemptResult
{
    Success = 1,
    AlreadyCheckedIn = 2,
    InvalidToken = 3,
    WrongEvent = 4,
    Cancelled = 5,
    CheckInUnavailable = 6
}
