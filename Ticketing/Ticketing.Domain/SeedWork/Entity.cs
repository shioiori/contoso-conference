using Eventbox.Shared.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eventbox.Ticketing.Domain.SeedWork
{
    public abstract class Entity<T> : IAuditableEntity
    {
        public T Id { get; protected set; }
        public DateTimeOffset CreatedDate { get; private set; }
        public DateTimeOffset? UpdatedDate { get; private set; }
        public string? CreatedBy { get; private set; }
        public string? UpdatedBy { get; private set; }

        private List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
        public IReadOnlyCollection <IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void MarkCreated(string? userId, DateTimeOffset utcNow)
        {
            CreatedDate = utcNow;
            CreatedBy = userId;
            UpdatedDate = null;
            UpdatedBy = null;
        }

        public void MarkUpdated(string? userId, DateTimeOffset utcNow)
        {
            UpdatedDate = utcNow;
            UpdatedBy = userId;
        }

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
