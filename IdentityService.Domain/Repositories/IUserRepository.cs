using IdentityService.Domain.Aggregates.User;
using IdentityService.Domain.ValueObjects;

namespace IdentityService.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(Guid tenantId, Email email, CancellationToken cancellationToken);
    Task<bool> ExistsByEmailAsync(Guid tenantId, Email email, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task UpdateAsync(User user, CancellationToken cancellationToken);
}
