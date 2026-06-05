using SharedKernel.Domain.Events;

namespace SharedKernel.Domain.Primitives;

public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> domainEvents = [];

    protected AggregateRoot(Guid id, Guid tenantId) : base(id, tenantId) { }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => domainEvents.Clear();
}
