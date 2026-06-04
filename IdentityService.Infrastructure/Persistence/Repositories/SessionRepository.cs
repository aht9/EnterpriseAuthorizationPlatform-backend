using System.Collections.Concurrent;
using IdentityService.Domain.Aggregates.Session;
using IdentityService.Domain.Repositories;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public sealed class SessionRepository : ISessionRepository
{
    private readonly ConcurrentDictionary<(Guid TenantId, Guid SessionId), Session> sessionsById = new();

    public Task AddAsync(Session session, CancellationToken cancellationToken)
    {
        if (!sessionsById.TryAdd((session.TenantId, session.Id), session)) throw new InvalidOperationException("Session already exists.");
        return Task.CompletedTask;
    }

    public Task<Session?> GetByIdAsync(Guid tenantId, Guid sessionId, CancellationToken cancellationToken)
    {
        sessionsById.TryGetValue((tenantId, sessionId), out var session);
        return Task.FromResult(session);
    }

    public Task<Session?> GetByRefreshTokenAsync(Guid tenantId, string refreshToken, CancellationToken cancellationToken) =>
        Task.FromResult(sessionsById.Values.FirstOrDefault(session => session.TenantId == tenantId && session.MatchesRefreshToken(refreshToken)));

    public Task UpdateAsync(Session session, CancellationToken cancellationToken)
    {
        sessionsById[(session.TenantId, session.Id)] = session;
        return Task.CompletedTask;
    }
}
