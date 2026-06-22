using Eventbox.Ticketing.Domain.Orders;
using Eventbox.Ticketing.Domain.Enums;

namespace Eventbox.UnitTests.Ticketing;

public class OrderTicketIssuingTests
{
    [Fact]
    public void Confirm_WhenOrderHasMultipleQuantity_ConfirmsOrder()
    {
        var ticketTypeId = Guid.NewGuid();
        var order = new Order(
            Guid.NewGuid(),
            userId: null,
            new PersonalInfo("Buyer", "buyer@example.com"),
            [new OrderItem(ticketTypeId, quantity: 3)],
            accessCode: "ABC12345",
            DateTimeOffset.UtcNow.AddMinutes(10));

        var changed = order.Confirm();

        Assert.True(changed);
        Assert.Equal(OrderState.Confirmed, order.OrderState);
        Assert.Single(order.OrderItems);
        Assert.Equal(3, order.OrderItems.Single().Quantity);
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
