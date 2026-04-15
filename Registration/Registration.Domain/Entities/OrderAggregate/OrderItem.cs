using Registration.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Text;

namespace Registration.Domain.Entities.OrderAggregate
{
    public class OrderItem : Entity<Guid>
    {
        public int SeatTypeId { get; private set; }
        public int Quantity { get; private set; }
    }
}
