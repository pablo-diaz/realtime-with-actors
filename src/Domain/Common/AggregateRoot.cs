using System;

using CSharpFunctionalExtensions;

namespace Domain.Common;

public abstract class AggregateRoot<T> : Entity<T> where T : IComparable<T>
{
    protected AggregateRoot(T id): base(id) { }
}