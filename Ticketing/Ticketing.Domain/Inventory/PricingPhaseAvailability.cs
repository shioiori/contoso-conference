namespace Eventbox.Ticketing.Domain.Entities.TicketAvailabilityAggregate;

public class PricingPhaseAvailability
{
    private PricingPhaseAvailability()
    {
        Name = default!;
    }

    public PricingPhaseAvailability(int id, string name, decimal price, DateTimeOffset? startTime, DateTimeOffset? endTime)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id), "Pricing phase id must be greater than zero.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Pricing phase name is required.", nameof(name));

        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

        if (startTime.HasValue && endTime.HasValue && endTime <= startTime)
            throw new ArgumentException("End time must be after start time.", nameof(endTime));

        Id = id;
        Name = name.Trim();
        Price = price;
        StartTime = startTime;
        EndTime = endTime;
    }

    public int Id { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public DateTimeOffset? StartTime { get; private set; }
    public DateTimeOffset? EndTime { get; private set; }

    public bool IsActive(DateTimeOffset utcNow)
        => (!StartTime.HasValue || StartTime <= utcNow)
            && (!EndTime.HasValue || utcNow < EndTime);
}
