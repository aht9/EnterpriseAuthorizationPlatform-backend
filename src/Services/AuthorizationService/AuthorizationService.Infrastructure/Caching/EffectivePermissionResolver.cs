using AuthorizationService.Application.Common.Abstractions;
using AuthorizationService.Domain.Repositories;
using AuthorizationService.Domain.Services;

namespace AuthorizationService.Infrastructure.Caching;

public sealed class EffectivePermissionResolver(IPermissionRepository permissions, IRoleAssignmentRepository assignments, IAuthorizationCache cache) : IEffectivePermissionResolver
{
    public async Task<IReadOnlyCollection<string>> ResolveAsync(Guid tenantId, Guid subjectId, CancellationToken cancellationToken)
    {
        var cached = await cache.GetEffectivePermissionsAsync(tenantId, subjectId, cancellationToken);
        if (cached is not null) return cached;
        var roleIds = await assignments.GetActiveRoleIdsIncludingInheritedAsync(tenantId, subjectId, cancellationToken);
        var effective = await permissions.GetActivePermissionKeysForRolesAsync(tenantId, roleIds, cancellationToken);
        await cache.SetEffectivePermissionsAsync(tenantId, subjectId, effective, cancellationToken);
        return effective;
    }
}
