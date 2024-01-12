using System;
using System.Collections.Generic;

using CSharpFunctionalExtensions;

namespace Domain.Common
{
    public abstract class AggregateRoot<T> : Entity<T> where T : IComparable<T>
    {
        private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

        protected AggregateRoot(T id): base(id) { }

        protected void RaiseDomainEvent(IDomainEvent domainEventToRaise)
        {
            this._domainEvents.Add(domainEventToRaise);
        }

        public void ClearDomainEvents()
        {
            this._domainEvents.Clear();
        }
    }
}