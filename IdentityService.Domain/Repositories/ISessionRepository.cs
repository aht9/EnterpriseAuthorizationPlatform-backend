using IdentityService.Domain.Aggregates.Session;

namespace IdentityService.Domain.Repositories;

public interface ISessionRepository
{
    Task<Session?> GetByIdAsync(Guid tenantId, Guid sessionId, CancellationToken cancellationToken);
    Task<Session?> GetByRefreshTokenAsync(Guid tenantId, string refreshToken, CancellationToken cancellationToken);
    Task AddAsync(Session session, CancellationToken cancellationToken);
    Task UpdateAsync(Session session, CancellationToken cancellationToken);
}
