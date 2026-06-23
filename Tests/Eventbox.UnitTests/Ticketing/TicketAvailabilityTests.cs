using Eventbox.Ticketing.Domain.Inventory;

namespace Eventbox.UnitTests.Ticketing;

public class TicketAvailabilityTests
{
    [Fact]
    public void Reserve_WhenQuantityExceedsRemaining_ThrowsAndKeepsRemaining()
    {
        var ticketType = new TicketTypeAvailability(Guid.NewGuid(), Guid.NewGuid(), quantity: 2);

        var exception = Assert.Throws<InvalidOperationException>(() => ticketType.Reserve(3));

        Assert.Equal("Not enough tickets remaining.", exception.Message);
        Assert.Equal(2, ticketType.Remaining);
    }

    [Fact]
    public void Release_WhenQuantityWouldExceedCapacity_CapsRemainingAtOriginalQuantity()
    {
        var ticketType = new TicketTypeAvailability(Guid.NewGuid(), Guid.NewGuid(), quantity: 2);

        ticketType.Reserve(1);
        ticketType.Release(10);

        Assert.Equal(2, ticketType.Remaining);
    }
}
