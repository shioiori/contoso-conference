using System;
using System.Collections.Generic;
using System.Text;

namespace Eventbox.Ticketing.Domain.SeedWork
{
    public interface IDomainEvent
    {
        public Guid Id { get; set; }
        public DateOnly Timestamp { get; set; }
    }
}
