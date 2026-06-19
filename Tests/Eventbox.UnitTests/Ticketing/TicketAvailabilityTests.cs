using Eventbox.Ticketing.Domain.Entities.TicketAvailabilityAggregate;

namespace Eventbox.UnitTests.Ticketing;

public class TicketAvailabilityTests
{
    [Fact]
    public void Reserve_WhenQuantityExceedsRemaining_RejectsAndKeepsRemainingTicketTypes()
    {
        var ticketType = new TicketTypeAvailability(ticketTypeId: 1, quantity: 2);
        var availability = new TicketAvailability(Guid.NewGuid(), [ticketType]);

        var exception = Assert.Throws<InvalidOperationException>(() => availability.Reserve(1, 3));

        Assert.Equal("Not enough tickets remaining.", exception.Message);
        Assert.Equal(2, ticketType.Remaining);
    }

    [Fact]
    public void Release_WhenQuantityWouldExceedCapacity_CapsRemainingAtOriginalQuantity()
    {
        var ticketType = new TicketTypeAvailability(ticketTypeId: 1, quantity: 2);
        var availability = new TicketAvailability(Guid.NewGuid(), [ticketType]);

        availability.Reserve(1, 1);
        availability.Release(1, 10);

        Assert.Equal(2, ticketType.Remaining);
    }
}
