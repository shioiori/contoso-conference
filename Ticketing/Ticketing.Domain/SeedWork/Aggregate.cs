using Eventbox.Ticketing.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Eventbox.Ticketing.Domain.SeedWork;
public abstract class Aggregate<T> : Entity<T>
{
}