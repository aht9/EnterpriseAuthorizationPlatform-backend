using AuthorizationService.Domain.Aggregates.Permission;
using AuthorizationService.Domain.Aggregates.PermissionGrant;
using AuthorizationService.Domain.Repositories;
using AuthorizationService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AuthorizationService.Infrastructure.Persistence.Repositories;

public sealed class PermissionRepository(AuthorizationDbContext dbContext) : IPermissionRepository
{
    public Task<Permission?> GetByIdAsync(Guid tenantId, Guid permissionId, CancellationToken cancellationToken) => dbContext.Permissions.AsNoTracking().FirstOrDefaultAsync(permission => permission.TenantId == tenantId && permission.Id == permissionId, cancellationToken);
    public Task<Permission?> GetByKeyAsync(Guid tenantId, PermissionKey key, CancellationToken cancellationToken) => dbContext.Permissions.AsNoTracking().FirstOrDefaultAsync(permission => permission.TenantId == tenantId && permission.Key == key, cancellationToken);
    public Task<bool> ExistsByKeyAsync(Guid tenantId, PermissionKey key, CancellationToken cancellationToken) => dbContext.Permissions.AsNoTracking().AnyAsync(permission => permission.TenantId == tenantId && permission.Key == key, cancellationToken);
    public async Task<IReadOnlyCollection<string>> GetActivePermissionKeysForRolesAsync(Guid tenantId, IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken) => await dbContext.PermissionGrants.AsNoTracking()
        .Where(grant => grant.TenantId == tenantId && roleIds.Contains(grant.RoleId) && grant.RevokedAtUtc == null)
        .Join(dbContext.Permissions.AsNoTracking().Where(permission => permission.TenantId == tenantId), grant => grant.PermissionId, permission => permission.Id, (_, permission) => permission.Key.Value)
        .Distinct()
        .ToArrayAsync(cancellationToken);
    public async Task AddAsync(Permission permission, CancellationToken cancellationToken) => await dbContext.Permissions.AddAsync(permission, cancellationToken);
    public async Task AddGrantAsync(PermissionGrant grant, CancellationToken cancellationToken) => await dbContext.PermissionGrants.AddAsync(grant, cancellationToken);
    public Task<PermissionGrant?> GetActiveGrantAsync(Guid tenantId, Guid roleId, Guid permissionId, CancellationToken cancellationToken) => dbContext.PermissionGrants.AsNoTracking().FirstOrDefaultAsync(grant => grant.TenantId == tenantId && grant.RoleId == roleId && grant.PermissionId == permissionId && grant.RevokedAtUtc == null, cancellationToken);
    public Task UpdateGrantAsync(PermissionGrant grant, CancellationToken cancellationToken) { dbContext.PermissionGrants.Update(grant); return Task.CompletedTask; }
}
