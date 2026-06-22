namespace Eventbox.Ticketing.Domain.Inventory;

public class PricingPhaseAvailability
{
    private PricingPhaseAvailability()
    {
        Name = default!;
    }

    public PricingPhaseAvailability(Guid id, string name, decimal price, DateTimeOffset? startTime, DateTimeOffset? endTime)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Pricing phase id must not be empty.", nameof(id));

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

    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }

    public bool IsActive(DateTimeOffset utcNow)
        => (!StartTime.HasValue || StartTime <= utcNow)
            && (!EndTime.HasValue || utcNow < EndTime);
}
