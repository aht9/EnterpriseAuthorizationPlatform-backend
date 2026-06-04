using System.Collections.Concurrent;
using IdentityService.Domain.Aggregates.User;
using IdentityService.Domain.Repositories;
using IdentityService.Domain.ValueObjects;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<(Guid TenantId, Guid UserId), User> usersById = new();

    public Task AddAsync(User user, CancellationToken cancellationToken)
    {
        if (!usersById.TryAdd((user.TenantId, user.Id), user)) throw new InvalidOperationException("User already exists.");
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByEmailAsync(Guid tenantId, Email email, CancellationToken cancellationToken) =>
        Task.FromResult(usersById.Values.Any(user => user.TenantId == tenantId && user.Email == email && !user.IsDeleted));

    public Task<User?> GetByEmailAsync(Guid tenantId, Email email, CancellationToken cancellationToken) =>
        Task.FromResult(usersById.Values.FirstOrDefault(user => user.TenantId == tenantId && user.Email == email && !user.IsDeleted));

    public Task<User?> GetByIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken)
    {
        usersById.TryGetValue((tenantId, userId), out var user);
        return Task.FromResult(user is { IsDeleted: false } ? user : null);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        usersById[(user.TenantId, user.Id)] = user;
        return Task.CompletedTask;
    }
}
