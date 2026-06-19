using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.EventManagement.EventApi.Enums;
using Eventbox.Shared.Exceptions;

namespace Eventbox.UnitTests.EventManagement;

public class EventAggregateTests
{
    [Fact]
    public void Event_WhenToEqualsFrom_Throws()
    {
        var at = new DateTimeOffset(2025, 6, 1, 10, 0, 0, TimeSpan.Zero);

        Assert.Throws<ArgumentException>(() =>
            new Event(Guid.NewGuid(), Guid.NewGuid(), "Fest", "fest", at, at, null, ""));
    }

    [Fact]
    public void Event_WhenToIsBeforeFrom_Throws()
    {
        var from = new DateTimeOffset(2025, 6, 1, 10, 0, 0, TimeSpan.Zero);
        var to   = from.AddHours(-1);

        Assert.Throws<ArgumentException>(() =>
            new Event(Guid.NewGuid(), Guid.NewGuid(), "Fest", "fest", from, to, null, ""));
    }

    [Fact]
    public void Event_Status_WhenPublished_IsPublished()
    {
        var ev = CreateFutureEvent();
        ev.Publish();

        Assert.Equal(EventStatus.Published, ev.Status);
    }

    [Fact]
    public void Event_Status_WhenUnpublishedAndFutureStart_IsDraft()
    {
        var ev = CreateFutureEvent();

        Assert.Equal(EventStatus.Draft, ev.Status);
    }

    [Fact]
    public void Event_Status_WhenPublishedThenUnpublishedAndPastStart_IsCancelled()
    {
        var past = DateTimeOffset.UtcNow.AddDays(-2);
        var ev = new Event(Guid.NewGuid(), Guid.NewGuid(), "Old", "old",
            past, past.AddHours(1), null, "");

        ev.Publish();
        ev.Unpublish();

        Assert.Equal(EventStatus.Cancelled, ev.Status);
    }

    [Fact]
    public void Event_Update_WhenToEqualsFrom_Throws()
    {
        var ev = CreateFutureEvent();
        var at = DateTimeOffset.UtcNow.AddDays(5);

        Assert.Throws<ArgumentException>(() => ev.Update("Name", at, at, null));
    }

    private static Event CreateFutureEvent()
    {
        var from = DateTimeOffset.UtcNow.AddDays(1);
        return new Event(Guid.NewGuid(), Guid.NewGuid(), "Fest", "fest",
            from, from.AddHours(2), null, "");
    }
}

public class PricingPhaseTests
{
    [Fact]
    public void PricingPhase_WhenPriceIsNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PricingPhase("Early Bird", -1, null, null));
    }

    [Fact]
    public void PricingPhase_WhenEndEqualsStart_Throws()
    {
        var at = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);

        Assert.Throws<ArgumentException>(() =>
            new PricingPhase("Phase", 0, at, at));
    }

    [Fact]
    public void PricingPhase_WhenEndIsBeforeStart_Throws()
    {
        var start = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);

        Assert.Throws<ArgumentException>(() =>
            new PricingPhase("Phase", 0, start, start.AddSeconds(-1)));
    }

    [Fact]
    public void PricingPhase_IsActive_WhenNoBounds_AlwaysActive()
    {
        var phase = new PricingPhase("Open", 50_000, null, null);

        Assert.True(phase.IsActive(DateTimeOffset.MinValue));
        Assert.True(phase.IsActive(DateTimeOffset.UtcNow));
        Assert.True(phase.IsActive(DateTimeOffset.MaxValue));
    }

    [Fact]
    public void PricingPhase_IsActive_WhenExactlyAtStartTime_IsActive()
    {
        var start = new DateTimeOffset(2025, 6, 1, 9, 0, 0, TimeSpan.Zero);
        var phase = new PricingPhase("On-sale", 100_000, start, null);

        Assert.True(phase.IsActive(start));
    }

    [Fact]
    public void PricingPhase_IsActive_WhenExactlyAtEndTime_IsNotActive()
    {
        // EndTime is exclusive: utcNow < EndTime
        var end = new DateTimeOffset(2025, 6, 10, 0, 0, 0, TimeSpan.Zero);
        var phase = new PricingPhase("Early", 80_000, null, end);

        Assert.False(phase.IsActive(end));
    }

    [Fact]
    public void PricingPhase_IsActive_WhenOneSecondBeforeEnd_IsActive()
    {
        var end = new DateTimeOffset(2025, 6, 10, 0, 0, 0, TimeSpan.Zero);
        var phase = new PricingPhase("Early", 80_000, null, end);

        Assert.True(phase.IsActive(end.AddSeconds(-1)));
    }

    [Fact]
    public void PricingPhase_Overlaps_WhenAdjacentEndToStart_DoesNotOverlap()
    {
        // phase1: [day1, day5), phase2: [day5, day10) — touching boundary, not overlapping
        var day1 = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var day5 = day1.AddDays(4);
        var day10 = day1.AddDays(9);

        var phase1 = new PricingPhase("Early", 80_000, day1, day5);
        var phase2 = new PricingPhase("Late", 120_000, day5, day10);

        Assert.False(phase1.Overlaps(phase2));
        Assert.False(phase2.Overlaps(phase1));
    }

    [Fact]
    public void PricingPhase_Overlaps_WhenOpenEndedAndOpenStarted_Overlaps()
    {
        // phase1: [day1, ∞), phase2: (∞, day5) — overlap from day1 to day5
        var day1 = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var day5 = day1.AddDays(4);

        var phase1 = new PricingPhase("After day1", 100_000, day1, null);
        var phase2 = new PricingPhase("Before day5", 80_000, null, day5);

        Assert.True(phase1.Overlaps(phase2));
    }

    [Fact]
    public void PricingPhase_Overlaps_WhenBothUnbounded_Overlaps()
    {
        var phase1 = new PricingPhase("A", 100_000, null, null);
        var phase2 = new PricingPhase("B", 200_000, null, null);

        Assert.True(phase1.Overlaps(phase2));
    }
}

public class TicketTypeTests
{
    private static readonly Guid EventId = Guid.NewGuid();

    [Fact]
    public void TicketType_WhenNoPricingPhases_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => CreateTicketType(phases: []));

        Assert.Equal("pricingPhases", ex.ParamName);
    }

    [Fact]
    public void TicketType_WhenMinPerOrderIsZero_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateTicketType(minPerOrder: 0));
    }

    [Fact]
    public void TicketType_WhenMaxPerOrderLessThanMin_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateTicketType(minPerOrder: 3, maxPerOrder: 2));
    }

    [Fact]
    public void TicketType_WhenMaxPerOrderExceedsQuota_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateTicketType(quota: 10, maxPerOrder: 11));
    }

    [Fact]
    public void TicketType_WhenAccessCodeVisibilityWithoutHash_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateTicketType(visibility: TicketVisibility.AccessCode, accessCodeHash: null));
    }

    [Fact]
    public void TicketType_WhenPricingPhasesOverlap_ThrowsValidationException()
    {
        var day1 = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var phase1 = new PricingPhase("A", 100_000, null, day1.AddDays(5));
        var phase2 = new PricingPhase("B", 80_000, day1.AddDays(3), null); // overlaps phase1

        Assert.Throws<ValidationApiException>(() =>
            CreateTicketType(phases: [phase1, phase2]));
    }

    [Fact]
    public void TicketType_CurrencyIsNormalizedToUpperCase()
    {
        var tt = CreateTicketType(currency: "vnd");

        Assert.Equal("VND", tt.Currency);
    }

    [Fact]
    public void TicketType_IncreaseQuota_WhenZero_Throws()
    {
        var tt = CreateTicketType();

        Assert.Throws<ArgumentOutOfRangeException>(() => tt.IncreaseQuota(0));
    }

    [Fact]
    public void TicketType_IncreaseQuota_WhenNegative_Throws()
    {
        var tt = CreateTicketType();

        Assert.Throws<ArgumentOutOfRangeException>(() => tt.IncreaseQuota(-5));
    }

    [Fact]
    public void TicketType_IncreaseQuota_AddsToExistingQuota()
    {
        var tt = CreateTicketType(quota: 50);

        tt.IncreaseQuota(30);

        Assert.Equal(80, tt.Quota);
    }

    private static PricingPhase DefaultPhase() => new("Standard", 100_000, null, null);

    private static TicketType CreateTicketType(
        int quota = 100,
        string currency = "VND",
        int minPerOrder = 1,
        int? maxPerOrder = null,
        TicketVisibility visibility = TicketVisibility.Public,
        string? accessCodeHash = null,
        IEnumerable<PricingPhase>? phases = null)
        => new(
            name: "General Admission",
            eventId: EventId,
            quota: quota,
            description: null,
            currency: currency,
            minPerOrder: minPerOrder,
            maxPerOrder: maxPerOrder,
            visibility: visibility,
            accessCodeHash: accessCodeHash,
            pricingPhases: phases ?? [DefaultPhase()]);
}
