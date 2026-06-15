using Eventbox.TicketingDomain.SeedWork;
using Eventbox.TicketingDomain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eventbox.TicketingDomain.Entities.OrderAggregate
{
    public class Order : Aggregate<Guid>
    {
        private readonly List<OrderItem> _orderItems = new();
        private readonly List<Ticket> _tickets = new();

        private Order()
        {
            PersonalInfo = null!;
        }

        public Order(Guid eventId, Guid? userId, PersonalInfo personalInfo, IEnumerable<OrderItem> orderItems, string accessCode, DateTimeOffset reservationExpiresAt)
        {
            var items = orderItems.ToList();
            if (eventId == Guid.Empty)
                throw new ArgumentException("Event id is required.", nameof(eventId));

            if (items.Count == 0)
                throw new ArgumentException("An order must have at least one item.", nameof(orderItems));

            Id = Guid.NewGuid();
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
        public IReadOnlyCollection<Ticket> Tickets => _tickets.AsReadOnly();

        public OrderState GetCurrentState(DateTimeOffset utcNow)
        {
            if (OrderState == OrderState.Pending && ReservationExpiresAt <= utcNow)
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

            EnsureTicketsIssued();
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

            foreach (var ticket in _tickets)
            {
                ticket.Cancel();
            }

            OrderState = OrderState.Cancelled;
            ReservationExpiresAt = null;
            return true;
        }

        public bool Expire(DateTimeOffset utcNow)
        {
            if (OrderState != OrderState.Pending)
                return false;

            if (ReservationExpiresAt > utcNow)
                return false;

            OrderState = OrderState.Expired;
            ReservationExpiresAt = null;
            return true;
        }

        private void EnsureTicketsIssued()
        {
            if (_tickets.Count > 0)
                return;

            var sequenceNumber = 1;
            foreach (var item in _orderItems)
            {
                for (var i = 0; i < item.Quantity; i++)
                {
                    _tickets.Add(new Ticket(EventId, item.TicketTypeId, sequenceNumber++));
                }
            }
        }
    }
}
