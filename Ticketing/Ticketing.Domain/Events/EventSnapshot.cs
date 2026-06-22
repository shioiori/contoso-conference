using Eventbox.Ticketing.Domain.SeedWork;

namespace Eventbox.Ticketing.Domain.Events;

public class EventSnapshot : Entity<Guid>
{
    private EventSnapshot()
    {
    }

    public EventSnapshot(Guid eventId, DateTimeOffset from, DateTimeOffset to, bool isPublished)
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
        IsPublished = isPublished;
    }

    public DateTimeOffset From { get; private set; }
    public DateTimeOffset To { get; private set; }
    public bool IsPublished { get; private set;  }

    public void Update(DateTimeOffset from, DateTimeOffset to, bool? isPublished)
    {
        from = from.ToUniversalTime();
        to = to.ToUniversalTime();

        if (to <= from)
            throw new ArgumentException("To must be after From.", nameof(to));

        From = from;
        To = to;
        if (isPublished.HasValue)
        {
            IsPublished = isPublished.Value;
        }
    }

    public void Publish() => IsPublished = true;

    public void Unpublish() => IsPublished = false;

    public bool IsCheckInAvailable(DateTimeOffset at)
        => at >= From && at <= To;

}
