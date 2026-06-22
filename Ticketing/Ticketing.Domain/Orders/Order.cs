using Eventbox.Shared.SeedWorks;
using Eventbox.Ticketing.Domain.Enums;

namespace Eventbox.Ticketing.Domain.Orders
{
    public class Order : Aggregate<Guid>
    {
        private readonly List<OrderItem> _orderItems = new();

        public Order(Guid eventId, Guid? userId, PersonalInfo personalInfo, 
            IEnumerable<OrderItem> orderItems, string accessCode, DateTimeOffset reservationExpiresAt) : base(Guid.NewGuid())
        {
            var items = orderItems.ToList();
            if (eventId == Guid.Empty)
                throw new ArgumentException("Event id is required.", nameof(eventId));

            if (items.Count == 0)
                throw new ArgumentException("An order must have at least one item.", nameof(orderItems));

            EventId = eventId;
            UserId = userId;
            OrderState = OrderState.Pending;
            AccessCode = accessCode;
            ReservationExpiresAt = reservationExpiresAt;
            PersonalInfo = personalInfo;
            _orderItems.AddRange(items);
        }

        public Guid EventId { get; private set; }
        public Guid? UserId { get; private set; }
        public OrderState OrderState { get; private set; }
        public string? AccessCode { get; private set; }
        public DateTimeOffset? ReservationExpiresAt { get; private set; }
        public PersonalInfo PersonalInfo { get; private set; }

        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

        public OrderState GetCurrentState(DateTimeOffset utcNow)
        {
            if ((OrderState is OrderState.Pending or OrderState.PaymentFailed) && ReservationExpiresAt <= utcNow)
                return OrderState.Expired;

            return OrderState;
        }

        public bool Confirm()
        {
            if (OrderState == OrderState.Confirmed)
                return false;

            if (OrderState is OrderState.Cancelled or OrderState.Expired)
                throw new InvalidOperationException("Cancelled or expired orders cannot be confirmed.");

            if (ReservationExpiresAt <= DateTimeOffset.UtcNow)
                throw new InvalidOperationException("Expired reservations cannot be confirmed.");

            OrderState = OrderState.Confirmed;
            ReservationExpiresAt = null;
            return true;
        }

        public bool Cancel()
        {
            if (OrderState == OrderState.Confirmed)
                throw new InvalidOperationException("Confirmed orders cannot be canceled.");

            if (OrderState is OrderState.Cancelled or OrderState.Expired)
                return false;

            OrderState = OrderState.Cancelled;
            ReservationExpiresAt = null;
            return true;
        }

        public bool MarkPaymentFailed()
        {
            if (OrderState is not (OrderState.Pending or OrderState.PaymentFailed))
                return false;

            OrderState = OrderState.PaymentFailed;
            return true;
        }

        public bool Expire(DateTimeOffset utcNow)
        {
            if (OrderState is not (OrderState.Pending or OrderState.PaymentFailed))
                return false;

            if (ReservationExpiresAt > utcNow)
                return false;

            OrderState = OrderState.Expired;
            ReservationExpiresAt = null;
            return true;
        }
    }
}
