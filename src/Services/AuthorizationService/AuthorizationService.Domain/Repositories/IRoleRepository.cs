using AuthorizationService.Domain.Aggregates.Role;
using AuthorizationService.Domain.ValueObjects;

namespace AuthorizationService.Domain.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken);
    Task<Role?> GetByNameAsync(Guid tenantId, RoleName name, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(Guid tenantId, RoleName name, CancellationToken cancellationToken);
    Task AddAsync(Role role, CancellationToken cancellationToken);
    Task UpdateAsync(Role role, CancellationToken cancellationToken);
}
