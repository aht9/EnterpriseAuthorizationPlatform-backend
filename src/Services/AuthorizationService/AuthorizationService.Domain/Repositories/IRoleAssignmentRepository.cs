using AuthorizationService.Domain.Aggregates.RoleAssignment;

namespace AuthorizationService.Domain.Repositories;

public interface IRoleAssignmentRepository
{
    Task<IReadOnlyCollection<RoleAssignment>> GetActiveForSubjectAsync(Guid tenantId, Guid subjectId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Guid>> GetActiveRoleIdsIncludingInheritedAsync(Guid tenantId, Guid subjectId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<string>> GetActiveRoleNamesForSubjectAsync(Guid tenantId, Guid subjectId, CancellationToken cancellationToken);
    Task<RoleAssignment?> GetActiveAsync(Guid tenantId, Guid subjectId, Guid roleId, CancellationToken cancellationToken);
    Task AddAsync(RoleAssignment assignment, CancellationToken cancellationToken);
    Task UpdateAsync(RoleAssignment assignment, CancellationToken cancellationToken);
}
