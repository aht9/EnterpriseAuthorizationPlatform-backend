using AuthorizationService.Application.Common.Abstractions;
using AuthorizationService.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthorizationService.Infrastructure.Caching;

public sealed class RedisAuthorizationCache(IMemoryCache cache, IOptions<AuthorizationCacheOptions> options, ILogger<RedisAuthorizationCache> logger) : IAuthorizationCache
{
    public Task<IReadOnlyCollection<string>?> GetEffectivePermissionsAsync(Guid tenantId, Guid subjectId, CancellationToken cancellationToken)
    {
        try
        {
            var value = cache.Get<IReadOnlyCollection<string>>(CacheKeyBuilder.EffectivePermissions(tenantId, subjectId));
            logger.LogInformation(value is null ? "Authorization cache miss tenant={TenantId} subject={SubjectId}" : "Authorization cache hit tenant={TenantId} subject={SubjectId}", tenantId, subjectId);
            return Task.FromResult(value);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Authorization cache read failed tenant={TenantId} subject={SubjectId}", tenantId, subjectId);
            return Task.FromResult<IReadOnlyCollection<string>?>(null);
        }
    }

    public Task SetEffectivePermissionsAsync(Guid tenantId, Guid subjectId, IReadOnlyCollection<string> permissions, CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled) return Task.CompletedTask;
        cache.Set(CacheKeyBuilder.EffectivePermissions(tenantId, subjectId), permissions, TimeSpan.FromSeconds(options.Value.EffectivePermissionsTtlSeconds));
        return Task.CompletedTask;
    }

    public Task<AuthorizationDecisionResult?> GetDecisionAsync(Guid tenantId, string decisionHash, CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled || !options.Value.DecisionCacheEnabled) return Task.FromResult<AuthorizationDecisionResult?>(null);
        return Task.FromResult(cache.Get<AuthorizationDecisionResult>(CacheKeyBuilder.Decision(tenantId, decisionHash)));
    }

    public Task SetDecisionAsync(Guid tenantId, string decisionHash, AuthorizationDecisionResult decision, CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled || !options.Value.DecisionCacheEnabled || !decision.IsAllowed) return Task.CompletedTask;
        cache.Set(CacheKeyBuilder.Decision(tenantId, decisionHash), decision, TimeSpan.FromSeconds(options.Value.DecisionCacheTtlSeconds));
        return Task.CompletedTask;
    }

    public Task InvalidateSubjectAsync(Guid tenantId, Guid subjectId, CancellationToken cancellationToken)
    {
        cache.Remove(CacheKeyBuilder.EffectivePermissions(tenantId, subjectId));
        return Task.CompletedTask;
    }

    public Task InvalidateTenantAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Tenant authorization cache invalidation requested tenant={TenantId}", tenantId);
        return Task.CompletedTask;
    }
}
