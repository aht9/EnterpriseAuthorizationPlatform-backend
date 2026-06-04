using IdentityService.Domain.Aggregates.Session;
using IdentityService.Domain.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityService.Api.Caching;

public sealed class CachedSessionRepository(ISessionRepository inner, IMemoryCache cache) : ISessionRepository
{
    private static readonly TimeSpan PositiveCacheTtl = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan NegativeCacheTtl = TimeSpan.FromSeconds(2);

    public async Task<Session?> GetByIdAsync(Guid tenantId, Guid sessionId, CancellationToken cancellationToken)
    {
        var key = SessionByIdKey(tenantId, sessionId);
        if (cache.TryGetValue(key, out Session? cached))
        {
            return cached;
        }

        var session = await inner.GetByIdAsync(tenantId, sessionId, cancellationToken);
        cache.Set(key, session, session is null ? NegativeCacheTtl : PositiveCacheTtl);
        return session;
    }

    public Task<Session?> GetByRefreshTokenAsync(Guid tenantId, string refreshToken, CancellationToken cancellationToken) =>
        inner.GetByRefreshTokenAsync(tenantId, refreshToken, cancellationToken);

    public async Task AddAsync(Session session, CancellationToken cancellationToken)
    {
        await inner.AddAsync(session, cancellationToken);
        cache.Set(SessionByIdKey(session.TenantId, session.Id), session, PositiveCacheTtl);
    }

    public async Task UpdateAsync(Session session, CancellationToken cancellationToken)
    {
        await inner.UpdateAsync(session, cancellationToken);
        cache.Set(SessionByIdKey(session.TenantId, session.Id), session, PositiveCacheTtl);
    }

    private static string SessionByIdKey(Guid tenantId, Guid sessionId) => $"identity:session:id:{tenantId:N}:{sessionId:N}";
}
