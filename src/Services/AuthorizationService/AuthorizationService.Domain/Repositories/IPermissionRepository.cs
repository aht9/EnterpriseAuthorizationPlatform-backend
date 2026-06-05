using AuthorizationService.Domain.Aggregates.Permission;
using AuthorizationService.Domain.Aggregates.PermissionGrant;
using AuthorizationService.Domain.ValueObjects;

namespace AuthorizationService.Domain.Repositories;

public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(Guid tenantId, Guid permissionId, CancellationToken cancellationToken);
    Task<Permission?> GetByKeyAsync(Guid tenantId, PermissionKey key, CancellationToken cancellationToken);
    Task<bool> ExistsByKeyAsync(Guid tenantId, PermissionKey key, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<string>> GetActivePermissionKeysForRolesAsync(Guid tenantId, IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken);
    Task AddAsync(Permission permission, CancellationToken cancellationToken);
    Task AddGrantAsync(PermissionGrant grant, CancellationToken cancellationToken);
    Task<PermissionGrant?> GetActiveGrantAsync(Guid tenantId, Guid roleId, Guid permissionId, CancellationToken cancellationToken);
    Task UpdateGrantAsync(PermissionGrant grant, CancellationToken cancellationToken);
}
