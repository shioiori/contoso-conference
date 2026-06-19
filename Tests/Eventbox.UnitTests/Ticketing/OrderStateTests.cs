using Eventbox.Ticketing.Domain.Entities.OrderAggregate;
using Eventbox.Ticketing.Domain.Enums;

namespace Eventbox.UnitTests.Ticketing;

public class OrderStateTests
{
    // ── Confirm ───────────────────────────────────────────────────────────

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_ReturnsFalse()
    {
        var order = PendingOrder();
        order.Confirm();

        var changed = order.Confirm();

        Assert.False(changed);
        Assert.Equal(OrderState.Confirmed, order.OrderState);
    }

    [Fact]
    public void Confirm_WhenCancelled_Throws()
    {
        var order = PendingOrder();
        order.Cancel();

        Assert.Throws<InvalidOperationException>(() => order.Confirm());
    }

    [Fact]
    public void Confirm_WhenReservationExpired_Throws()
    {
        var order = OrderExpiringAt(DateTimeOffset.UtcNow.AddSeconds(-1));

        Assert.Throws<InvalidOperationException>(() => order.Confirm());
    }

    // ── Cancel ────────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_WhenConfirmed_Throws()
    {
        var order = PendingOrder();
        order.Confirm();

        Assert.Throws<InvalidOperationException>(() => order.Cancel());
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ReturnsFalse()
    {
        var order = PendingOrder();
        order.Cancel();

        var changed = order.Cancel();

        Assert.False(changed);
    }

    [Fact]
    public void Cancel_WhenPending_CancelsSuccessfully()
    {
        var order = PendingOrder();

        var changed = order.Cancel();

        Assert.True(changed);
        Assert.Equal(OrderState.Cancelled, order.OrderState);
    }

    // ── Expire ────────────────────────────────────────────────────────────

    [Fact]
    public void Expire_WhenBeforeExpiryTime_ReturnsFalse()
    {
        var order = OrderExpiringAt(DateTimeOffset.UtcNow.AddMinutes(5));

        var expired = order.Expire(DateTimeOffset.UtcNow);

        Assert.False(expired);
        Assert.Equal(OrderState.Pending, order.OrderState);
    }

    [Fact]
    public void Expire_WhenExactlyAtExpiryTime_Expires()
    {
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(-1); // 1s in the past to avoid flakiness
        var order = OrderExpiringAt(expiresAt);

        var expired = order.Expire(expiresAt);

        Assert.True(expired);
        Assert.Equal(OrderState.Expired, order.OrderState);
    }

    [Fact]
    public void Expire_WhenConfirmed_ReturnsFalse()
    {
        var order = PendingOrder();
        order.Confirm();

        var expired = order.Expire(DateTimeOffset.UtcNow.AddDays(1));

        Assert.False(expired);
        Assert.Equal(OrderState.Confirmed, order.OrderState);
    }

    [Fact]
    public void Expire_WhenPaymentFailed_Expires()
    {
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(-1);
        var order = OrderExpiringAt(expiresAt);
        order.MarkPaymentFailed();

        var expired = order.Expire(expiresAt);

        Assert.True(expired);
        Assert.Equal(OrderState.Expired, order.OrderState);
    }

    // ── GetCurrentState ───────────────────────────────────────────────────

    [Fact]
    public void GetCurrentState_WhenPendingAndPastExpiry_ReturnsExpired()
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        var order = OrderExpiringAt(expiresAt);

        var state = order.GetCurrentState(DateTimeOffset.UtcNow);

        Assert.Equal(OrderState.Expired, state);
    }

    [Fact]
    public void GetCurrentState_WhenPendingAndNotExpired_ReturnsPending()
    {
        var order = PendingOrder();

        var state = order.GetCurrentState(DateTimeOffset.UtcNow);

        Assert.Equal(OrderState.Pending, state);
    }

    [Fact]
    public void GetCurrentState_WhenConfirmed_ReturnsConfirmed()
    {
        var order = PendingOrder();
        order.Confirm();

        // Even if we pass a time far in the future, Confirmed orders never expire
        var state = order.GetCurrentState(DateTimeOffset.MaxValue);

        Assert.Equal(OrderState.Confirmed, state);
    }

    // ── MarkPaymentFailed ─────────────────────────────────────────────────

    [Fact]
    public void MarkPaymentFailed_WhenConfirmed_ReturnsFalse()
    {
        var order = PendingOrder();
        order.Confirm();

        var marked = order.MarkPaymentFailed();

        Assert.False(marked);
        Assert.Equal(OrderState.Confirmed, order.OrderState);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static Order PendingOrder()
        => OrderExpiringAt(DateTimeOffset.UtcNow.AddMinutes(15));

    private static Order OrderExpiringAt(DateTimeOffset expiresAt)
        => new(
            Guid.NewGuid(),
            userId: null,
            new PersonalInfo("Buyer", "buyer@example.com"),
            [new OrderItem(ticketTypeId: 1, quantity: 1)],
            accessCode: "TESTCODE",
            expiresAt);
}
