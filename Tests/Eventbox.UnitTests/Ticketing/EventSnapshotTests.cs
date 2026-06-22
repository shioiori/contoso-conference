using Eventbox.Ticketing.Domain.Events;

namespace Eventbox.UnitTests.Ticketing;

public class EventSnapshotTests
{
    private static readonly DateTimeOffset From = new(2025, 6, 1, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset To   = new(2025, 6, 1, 21, 0, 0, TimeSpan.Zero);

    private static EventSnapshot CreateSnapshot() => new(Guid.NewGuid(), From, To, isPublished: true);

    [Fact]
    public void IsCheckInAvailable_WhenExactlyAtFrom_ReturnsTrue()
    {
        Assert.True(CreateSnapshot().IsCheckInAvailable(From));
    }

    [Fact]
    public void IsCheckInAvailable_WhenExactlyAtTo_ReturnsTrue()
    {
        // To is inclusive
        Assert.True(CreateSnapshot().IsCheckInAvailable(To));
    }

    [Fact]
    public void IsCheckInAvailable_WhenOneSecondBeforeFrom_ReturnsFalse()
    {
        Assert.False(CreateSnapshot().IsCheckInAvailable(From.AddSeconds(-1)));
    }

    [Fact]
    public void IsCheckInAvailable_WhenOneSecondAfterTo_ReturnsFalse()
    {
        Assert.False(CreateSnapshot().IsCheckInAvailable(To.AddSeconds(1)));
    }

    [Fact]
    public void IsCheckInAvailable_WhenBetweenFromAndTo_ReturnsTrue()
    {
        var mid = From + TimeSpan.FromTicks((To - From).Ticks / 2);

        Assert.True(CreateSnapshot().IsCheckInAvailable(mid));
    }

    [Fact]
    public void EventSnapshot_WhenToEqualsFrom_Throws()
    {
        var at = From;

        Assert.Throws<ArgumentException>(() => new EventSnapshot(Guid.NewGuid(), at, at, isPublished: true));
    }

    [Fact]
    public void EventSnapshot_WhenToIsBeforeFrom_Throws()
    {
        Assert.Throws<ArgumentException>(() => new EventSnapshot(Guid.NewGuid(), To, From, isPublished: true));
    }
}
