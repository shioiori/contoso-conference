using Eventbox.TicketingDomain.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Eventbox.TicketingDomain.SeedWork;
public abstract class Aggregate<T> : Entity<T>
{
}