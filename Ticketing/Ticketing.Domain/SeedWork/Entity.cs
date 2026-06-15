using System;
using System.Collections.Generic;
using System.Text;

namespace Eventbox.TicketingDomain.SeedWork
{
    public abstract class Entity<T>
    {
        public T Id { get; protected set; }
        private List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
        public IReadOnlyCollection <IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void RemoveDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
