using Eventbox.Ticketing.Domain.Entities.OrderAggregate;
using Eventbox.Ticketing.Domain.Enums;

namespace Eventbox.UnitTests.Registration;

public class OrderTicketIssuingTests
{
    [Fact]
    public void Confirm_WhenOrderHasMultipleQuantity_IssuesOneTicketPerQuantity()
    {
        var order = new Order(
            Guid.NewGuid(),
            userId: null,
            new PersonalInfo("Buyer", "buyer@example.com"),
            [new OrderItem(ticketTypeId: 7, quantity: 3)],
            accessCode: "ABC12345",
            DateTimeOffset.UtcNow.AddMinutes(10));

        var changed = order.Confirm();

        Assert.True(changed);
        Assert.Equal(OrderState.Confirmed, order.OrderState);
        Assert.Equal(3, order.Tickets.Count);
        Assert.All(order.Tickets, ticket => Assert.Equal(7, ticket.TicketTypeId));
        Assert.Equal([1, 2, 3], order.Tickets.Select(ticket => ticket.SequenceNumber));
    }

    [Fact]
    public void PersonalInfo_WhenNameIsBlank_AllowsOptionalName()
    {
        var personalInfo = new PersonalInfo("   ", "buyer@example.com");

        Assert.Equal(string.Empty, personalInfo.Name);
        Assert.Equal("buyer@example.com", personalInfo.Email);
    }

    [Fact]
    public void PersonalInfo_WhenEmailFormatIsInvalid_RejectsEmail()
    {
        var exception = Assert.Throws<ArgumentException>(() => new PersonalInfo("Buyer", "not-an-email"));

        Assert.Equal("email", exception.ParamName);
    }
}
