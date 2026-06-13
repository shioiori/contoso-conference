using Eventbox.Registration.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eventbox.Registration.Domain.Entities.OrderAggregate
{
    public class OrderItem : Entity<Guid>
    {
        private OrderItem()
        {
        }

        public OrderItem(int ticketTypeId, int quantity)
        {
            if (ticketTypeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(ticketTypeId), "Ticket type id must be greater than zero.");

            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

            Id = Guid.NewGuid();
            TicketTypeId = ticketTypeId;
            Quantity = quantity;
        }

        public int TicketTypeId { get; private set; }
        public int Quantity { get; private set; }
    }
}
