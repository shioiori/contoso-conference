using Registration.Domain.Entities.SeatAvailabilityAggregate;
using System;
using System.Collections.Generic;
using System.Text;

namespace Registration.Application.Queries
{
    public interface ISeatAvailabilityQueries
    {
        Task<SeatAvailability> GetSeatAvailability(Guid conferenceId);
    }
}
