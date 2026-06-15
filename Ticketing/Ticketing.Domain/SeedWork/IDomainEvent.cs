using System;
using System.Collections.Generic;
using System.Text;

namespace Eventbox.TicketingDomain.SeedWork
{
    public interface IDomainEvent
    {
        public Guid Id { get; set; }
        public DateOnly Timestamp { get; set; }
    }
}
