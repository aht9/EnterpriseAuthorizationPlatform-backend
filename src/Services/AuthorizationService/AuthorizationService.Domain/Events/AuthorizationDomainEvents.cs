using SharedKernel.Domain.Events;

namespace AuthorizationService.Domain.Events;

public abstract record AuthorizationDomainEvent(Guid TenantId, Guid CorrelationId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
    public int Version { get; } = 1;
}

public sealed record RoleCreatedDomainEvent(Guid RoleId, Guid TenantIdValue, string Name, Guid CorrelationIdValue) : AuthorizationDomainEvent(TenantIdValue, CorrelationIdValue);
public sealed record RoleActivatedDomainEvent(Guid RoleId, Guid TenantIdValue, Guid CorrelationIdValue) : AuthorizationDomainEvent(TenantIdValue, CorrelationIdValue);
public sealed record RoleDeactivatedDomainEvent(Guid RoleId, Guid TenantIdValue, Guid CorrelationIdValue) : AuthorizationDomainEvent(TenantIdValue, CorrelationIdValue);
public sealed record RoleAssignedDomainEvent(Guid AssignmentId, Guid SubjectId, Guid RoleId, Guid TenantIdValue, Guid CorrelationIdValue) : AuthorizationDomainEvent(TenantIdValue, CorrelationIdValue);
public sealed record RoleRevokedDomainEvent(Guid AssignmentId, Guid SubjectId, Guid RoleId, Guid TenantIdValue, Guid CorrelationIdValue) : AuthorizationDomainEvent(TenantIdValue, CorrelationIdValue);
public sealed record PermissionCreatedDomainEvent(Guid PermissionId, Guid TenantIdValue, string Key, Guid CorrelationIdValue) : AuthorizationDomainEvent(TenantIdValue, CorrelationIdValue);
public sealed record PermissionGrantedDomainEvent(Guid GrantId, Guid RoleId, Guid PermissionId, Guid TenantIdValue, Guid CorrelationIdValue) : AuthorizationDomainEvent(TenantIdValue, CorrelationIdValue);
public sealed record PermissionRevokedDomainEvent(Guid GrantId, Guid RoleId, Guid PermissionId, Guid TenantIdValue, Guid CorrelationIdValue) : AuthorizationDomainEvent(TenantIdValue, CorrelationIdValue);
public sealed record AuthorizationEvaluatedDomainEvent(Guid DecisionId, Guid SubjectId, string Action, string ResourceType, string ResourceId, bool IsAllowed, string ReasonCode, Guid TenantIdValue, Guid CorrelationIdValue) : AuthorizationDomainEvent(TenantIdValue, CorrelationIdValue);
