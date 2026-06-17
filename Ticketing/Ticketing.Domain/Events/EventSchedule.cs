using Eventbox.Ticketing.Domain.SeedWork;

namespace Eventbox.Ticketing.Domain.Entities;

public class EventSchedule : Entity<Guid>
{
    private EventSchedule()
    {
    }

    public EventSchedule(Guid eventId, DateTimeOffset from, DateTimeOffset to)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("Event id is required.", nameof(eventId));

        from = from.ToUniversalTime();
        to = to.ToUniversalTime();

        if (to <= from)
            throw new ArgumentException("To must be after From.", nameof(to));

        Id = eventId;
        From = from;
        To = to;
    }

    public DateTimeOffset From { get; private set; }
    public DateTimeOffset To { get; private set; }

    public void Update(DateTimeOffset from, DateTimeOffset to)
    {
        from = from.ToUniversalTime();
        to = to.ToUniversalTime();

        if (to <= from)
            throw new ArgumentException("To must be after From.", nameof(to));

        From = from;
        To = to;
    }

    public bool IsCheckInAvailable(DateTimeOffset at)
        => at >= From && at <= To;
}
