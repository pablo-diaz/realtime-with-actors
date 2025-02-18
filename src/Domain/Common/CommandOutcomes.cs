using System.Collections.Generic;

namespace Domain.Common;

public record CommandOutcome(IReadOnlyList<IDomainEvent> DomainEventsRaised)
{
    public static CommandOutcome WithNoEvents() => new(DomainEventsRaised: []);
}

public sealed record CommandOutcomeWithAggregate<T>(IReadOnlyList<IDomainEvent> DomainEventsRaised, T AggregateResult) : CommandOutcome(DomainEventsRaised) where T : AggregateRoot<string>;
