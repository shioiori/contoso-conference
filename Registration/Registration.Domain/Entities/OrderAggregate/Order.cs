using EventSourcing.SeedWork;
using Registration.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Registration.Domain.Entities.OrderAggregate
{
    public class Order : Aggregate<Guid>
    {
        public Guid ConferenceId { get; set; }
        public Guid UserId { get; set; }
        public OrderState OrderState { get; set; }
        public string? AccessCode { get; set; }
        public DateTime? ReservationExpirationDate { get; set; }
        public PersonalInfo PersonalInfo { get; set; }

        private readonly List<OrderItem> _orderItems;
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();
    }
}
