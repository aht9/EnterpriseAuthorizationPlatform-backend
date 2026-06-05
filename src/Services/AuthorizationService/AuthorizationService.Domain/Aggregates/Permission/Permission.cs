using AuthorizationService.Domain.Events;
using AuthorizationService.Domain.ValueObjects;
using SharedKernel.Domain.Guards;
using SharedKernel.Domain.Primitives;

namespace AuthorizationService.Domain.Aggregates.Permission;

public sealed class Permission : AggregateRoot
{
    private Permission(Guid id, Guid tenantId, PermissionKey key, AuthorizationAction action, string resourceType, string? description) : base(id, tenantId)
    {
        Key = key;
        Action = action;
        ResourceType = Guard.NotEmpty(resourceType, nameof(resourceType));
        Description = description;
    }

    public PermissionKey Key { get; private set; }
    public AuthorizationAction Action { get; private set; }
    public string ResourceType { get; private set; }
    public string? Description { get; private set; }

    public static Permission Create(Guid tenantId, PermissionKey key, AuthorizationAction action, string resourceType, string? description, Guid correlationId)
    {
        var permission = new Permission(Guid.NewGuid(), tenantId, key, action, resourceType, description);
        permission.RaiseDomainEvent(new PermissionCreatedDomainEvent(permission.Id, tenantId, key.Value, correlationId));
        return permission;
    }
}
