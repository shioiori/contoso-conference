using Eventbox.Registration.Domain.Entities.SeatAvailabilityAggregate;

namespace Eventbox.UnitTests.Registration;

public class SeatAvailabilityTests
{
    [Fact]
    public void Reserve_WhenQuantityExceedsRemaining_RejectsAndKeepsRemainingSeats()
    {
        var ticketType = new TicketTypeAvailability(ticketTypeId: 1, quantity: 2);
        var availability = new SeatAvailability(Guid.NewGuid(), [ticketType]);

        var exception = Assert.Throws<InvalidOperationException>(() => availability.Reserve(1, 3));

        Assert.Equal("Not enough seats remaining.", exception.Message);
        Assert.Equal(2, ticketType.Remaining);
    }

    [Fact]
    public void Release_WhenQuantityWouldExceedCapacity_CapsRemainingAtOriginalQuantity()
    {
        var ticketType = new TicketTypeAvailability(ticketTypeId: 1, quantity: 2);
        var availability = new SeatAvailability(Guid.NewGuid(), [ticketType]);

        availability.Reserve(1, 1);
        availability.Release(1, 10);

        Assert.Equal(2, ticketType.Remaining);
    }
}
