using AuthorizationService.Domain.Events;
using SharedKernel.Domain.Guards;
using SharedKernel.Domain.Primitives;

namespace AuthorizationService.Domain.Aggregates.RoleAssignment;

public sealed class RoleAssignment : AggregateRoot
{
    private RoleAssignment(Guid id, Guid tenantId, Guid subjectId, Guid roleId, Guid assignedBy) : base(id, tenantId)
    {
        SubjectId = Guard.NotEmpty(subjectId, nameof(subjectId));
        RoleId = Guard.NotEmpty(roleId, nameof(roleId));
        AssignedBy = Guard.NotEmpty(assignedBy, nameof(assignedBy));
        AssignedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid SubjectId { get; private set; }
    public Guid RoleId { get; private set; }
    public Guid AssignedBy { get; private set; }
    public DateTimeOffset AssignedAtUtc { get; private set; }
    public DateTimeOffset? RevokedAtUtc { get; private set; }
    public bool IsActive => RevokedAtUtc is null;

    public static RoleAssignment Assign(Guid tenantId, Guid subjectId, Guid roleId, Guid assignedBy, Guid correlationId)
    {
        var assignment = new RoleAssignment(Guid.NewGuid(), tenantId, subjectId, roleId, assignedBy);
        assignment.RaiseDomainEvent(new RoleAssignedDomainEvent(assignment.Id, subjectId, roleId, tenantId, correlationId));
        return assignment;
    }

    public void Revoke(Guid correlationId)
    {
        if (RevokedAtUtc is not null) return;
        RevokedAtUtc = DateTimeOffset.UtcNow;
        MarkUpdated();
        RaiseDomainEvent(new RoleRevokedDomainEvent(Id, SubjectId, RoleId, TenantId, correlationId));
    }
}
