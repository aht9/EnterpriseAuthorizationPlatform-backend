using AuthorizationService.Domain.Events;
using AuthorizationService.Domain.ValueObjects;
using SharedKernel.Domain.Primitives;

namespace AuthorizationService.Domain.Aggregates.Role;

public sealed class Role : AggregateRoot
{
    private Role(Guid id, Guid tenantId, RoleName name, string? description, Guid? parentRoleId) : base(id, tenantId)
    {
        Name = name;
        Description = description;
        ParentRoleId = parentRoleId;
        Status = RoleStatus.Active;
    }

    public RoleName Name { get; private set; }
    public string? Description { get; private set; }
    public RoleStatus Status { get; private set; }
    public Guid? ParentRoleId { get; private set; }
    public bool IsActive => Status == RoleStatus.Active;

    public static Role Create(Guid tenantId, RoleName name, string? description, Guid? parentRoleId, Guid correlationId)
    {
        if (parentRoleId == Guid.Empty) throw new ArgumentException("ParentRoleId cannot be empty.", nameof(parentRoleId));
        var role = new Role(Guid.NewGuid(), tenantId, name, description, parentRoleId);
        if (role.Id == parentRoleId) throw new InvalidOperationException("Role cannot inherit from itself.");
        role.RaiseDomainEvent(new RoleCreatedDomainEvent(role.Id, tenantId, name.Value, correlationId));
        return role;
    }

    public void Activate(Guid correlationId)
    {
        if (Status == RoleStatus.Active) return;
        Status = RoleStatus.Active;
        MarkUpdated();
        RaiseDomainEvent(new RoleActivatedDomainEvent(Id, TenantId, correlationId));
    }

    public void Deactivate(Guid correlationId)
    {
        if (Status == RoleStatus.Inactive) return;
        Status = RoleStatus.Inactive;
        MarkUpdated();
        RaiseDomainEvent(new RoleDeactivatedDomainEvent(Id, TenantId, correlationId));
    }
}
