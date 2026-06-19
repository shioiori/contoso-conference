using Eventbox.Ticketing.Domain.Entities;

namespace Eventbox.UnitTests.Ticketing;

public class EventScheduleTests
{
    private static readonly DateTimeOffset From = new(2025, 6, 1, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset To   = new(2025, 6, 1, 21, 0, 0, TimeSpan.Zero);

    private static EventSchedule CreateSchedule() => new(Guid.NewGuid(), From, To);

    [Fact]
    public void IsCheckInAvailable_WhenExactlyAtFrom_ReturnsTrue()
    {
        Assert.True(CreateSchedule().IsCheckInAvailable(From));
    }

    [Fact]
    public void IsCheckInAvailable_WhenExactlyAtTo_ReturnsTrue()
    {
        // To is inclusive
        Assert.True(CreateSchedule().IsCheckInAvailable(To));
    }

    [Fact]
    public void IsCheckInAvailable_WhenOneSecondBeforeFrom_ReturnsFalse()
    {
        Assert.False(CreateSchedule().IsCheckInAvailable(From.AddSeconds(-1)));
    }

    [Fact]
    public void IsCheckInAvailable_WhenOneSecondAfterTo_ReturnsFalse()
    {
        Assert.False(CreateSchedule().IsCheckInAvailable(To.AddSeconds(1)));
    }

    [Fact]
    public void IsCheckInAvailable_WhenBetweenFromAndTo_ReturnsTrue()
    {
        var mid = From + TimeSpan.FromTicks((To - From).Ticks / 2);

        Assert.True(CreateSchedule().IsCheckInAvailable(mid));
    }

    [Fact]
    public void EventSchedule_WhenToEqualsFrom_Throws()
    {
        var at = From;

        Assert.Throws<ArgumentException>(() => new EventSchedule(Guid.NewGuid(), at, at));
    }

    [Fact]
    public void EventSchedule_WhenToIsBeforeFrom_Throws()
    {
        Assert.Throws<ArgumentException>(() => new EventSchedule(Guid.NewGuid(), To, From));
    }
}
