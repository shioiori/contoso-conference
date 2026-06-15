using Eventbox.EventManagement.EventApi.Domains.Common;

namespace Eventbox.EventManagement.EventApi.Domains;

public class PricingPhase : Entity<int>
{
    private PricingPhase()
    {
        Name = default!;
    }

    public PricingPhase(string name, decimal price, DateTimeOffset? startTime, DateTimeOffset? endTime)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Pricing phase name is required.", nameof(name));

        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

        if (startTime.HasValue && endTime.HasValue && endTime <= startTime)
            throw new ArgumentException("End time must be after start time.", nameof(endTime));

        Name = name.Trim();
        Price = price;
        StartTime = startTime;
        EndTime = endTime;
    }

    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public DateTimeOffset? StartTime { get; private set; }
    public DateTimeOffset? EndTime { get; private set; }

    public bool IsActive(DateTimeOffset utcNow)
        => (!StartTime.HasValue || StartTime <= utcNow)
            && (!EndTime.HasValue || utcNow < EndTime);

    public bool Overlaps(PricingPhase other)
    {
        var start = StartTime ?? DateTimeOffset.MinValue;
        var end = EndTime ?? DateTimeOffset.MaxValue;
        var otherStart = other.StartTime ?? DateTimeOffset.MinValue;
        var otherEnd = other.EndTime ?? DateTimeOffset.MaxValue;

        return start < otherEnd && otherStart < end;
    }
}
