using System;
using System.Collections.Generic;
using System.Text;

namespace Registration.Domain.SeedWork
{
    public interface IDomainEvent
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
