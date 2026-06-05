namespace AuthorizationService.Infrastructure.Caching;

public static class CacheKeyBuilder
{
    public static string EffectivePermissions(Guid tenantId, Guid subjectId) => $"authz:{tenantId}:subject:{subjectId}:effective-permissions";
    public static string Decision(Guid tenantId, string hash) => $"authz:{tenantId}:decision:{hash}";
}
