using Eventbox.Ticketing.Domain.Inventory;

namespace Eventbox.UnitTests.Ticketing;

public class TicketAvailabilityTests
{
    [Fact]
    public void Reserve_WhenQuantityExceedsRemaining_RejectsAndKeepsRemainingTicketTypes()
    {
        var ticketTypeId = Guid.NewGuid();
        var ticketType = new TicketTypeAvailability(ticketTypeId, quantity: 2);
        var availability = new TicketAvailability(Guid.NewGuid(), [ticketType]);

        var exception = Assert.Throws<InvalidOperationException>(() => availability.Reserve(ticketTypeId, 3));

        Assert.Equal("Not enough tickets remaining.", exception.Message);
        Assert.Equal(2, ticketType.Remaining);
    }

    [Fact]
    public void Release_WhenQuantityWouldExceedCapacity_CapsRemainingAtOriginalQuantity()
    {
        var ticketTypeId = Guid.NewGuid();
        var ticketType = new TicketTypeAvailability(ticketTypeId, quantity: 2);
        var availability = new TicketAvailability(Guid.NewGuid(), [ticketType]);

        availability.Reserve(ticketTypeId, 1);
        availability.Release(ticketTypeId, 10);

        Assert.Equal(2, ticketType.Remaining);
    }
}
