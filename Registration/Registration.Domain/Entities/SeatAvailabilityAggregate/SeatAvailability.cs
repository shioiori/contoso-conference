using Registration.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Text;

namespace Registration.Domain.Entities.SeatAvailabilityAggregate
{
    public class SeatAvailability : Entity<Guid>
    {
        public IEnumerable<SeatType> SeatTypes { get; set; }
    }
}
