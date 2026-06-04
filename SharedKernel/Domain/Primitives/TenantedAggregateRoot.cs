namespace SharedKernel.Domain.Primitives;

public abstract class TenantedAggregateRoot(Guid id, Guid tenantId) : AggregateRoot(id, tenantId);
