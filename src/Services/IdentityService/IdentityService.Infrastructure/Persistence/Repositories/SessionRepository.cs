using IdentityService.Domain.Aggregates.Session;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public sealed class SessionRepository(IdentityDbContext dbContext) : ISessionRepository
{
    public Task<Session?> GetByIdAsync(Guid tenantId, Guid sessionId, CancellationToken cancellationToken) =>
        dbContext.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(session => session.TenantId == tenantId && session.Id == sessionId, cancellationToken);

    public Task<Session?> GetByRefreshTokenAsync(Guid tenantId, string refreshToken, CancellationToken cancellationToken) =>
        dbContext.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(session => session.TenantId == tenantId && session.RefreshTokenHash == RefreshToken.Hash(refreshToken), cancellationToken);

    public async Task AddAsync(Session session, CancellationToken cancellationToken)
    {
        await dbContext.Sessions.AddAsync(session, cancellationToken);
    }

    public Task UpdateAsync(Session session, CancellationToken cancellationToken)
    {
        dbContext.Sessions.Update(session);
        return Task.CompletedTask;
    }
}
