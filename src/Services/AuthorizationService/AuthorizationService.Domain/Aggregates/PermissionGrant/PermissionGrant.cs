using AuthorizationService.Domain.Events;
using SharedKernel.Domain.Guards;
using SharedKernel.Domain.Primitives;

namespace AuthorizationService.Domain.Aggregates.PermissionGrant;

public sealed class PermissionGrant : AggregateRoot
{
    private PermissionGrant(Guid id, Guid tenantId, Guid roleId, Guid permissionId, Guid grantedBy) : base(id, tenantId)
    {
        RoleId = Guard.NotEmpty(roleId, nameof(roleId));
        PermissionId = Guard.NotEmpty(permissionId, nameof(permissionId));
        GrantedBy = Guard.NotEmpty(grantedBy, nameof(grantedBy));
        GrantedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
    public Guid GrantedBy { get; private set; }
    public DateTimeOffset GrantedAtUtc { get; private set; }
    public DateTimeOffset? RevokedAtUtc { get; private set; }
    public bool IsActive => RevokedAtUtc is null;

    public static PermissionGrant Grant(Guid tenantId, Guid roleId, Guid permissionId, Guid grantedBy, Guid correlationId)
    {
        var grant = new PermissionGrant(Guid.NewGuid(), tenantId, roleId, permissionId, grantedBy);
        grant.RaiseDomainEvent(new PermissionGrantedDomainEvent(grant.Id, roleId, permissionId, tenantId, correlationId));
        return grant;
    }

    public void Revoke(Guid correlationId)
    {
        if (RevokedAtUtc is not null) return;
        RevokedAtUtc = DateTimeOffset.UtcNow;
        MarkUpdated();
        RaiseDomainEvent(new PermissionRevokedDomainEvent(Id, RoleId, PermissionId, TenantId, correlationId));
    }
}
