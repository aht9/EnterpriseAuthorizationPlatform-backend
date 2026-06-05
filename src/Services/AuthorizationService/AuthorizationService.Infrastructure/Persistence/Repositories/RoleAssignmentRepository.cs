using AuthorizationService.Domain.Aggregates.RoleAssignment;
using AuthorizationService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AuthorizationService.Infrastructure.Persistence.Repositories;

public sealed class RoleAssignmentRepository(AuthorizationDbContext dbContext) : IRoleAssignmentRepository
{
    public async Task<IReadOnlyCollection<RoleAssignment>> GetActiveForSubjectAsync(Guid tenantId, Guid subjectId, CancellationToken cancellationToken) => await dbContext.RoleAssignments.AsNoTracking().Where(assignment => assignment.TenantId == tenantId && assignment.SubjectId == subjectId && assignment.RevokedAtUtc == null).ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Guid>> GetActiveRoleIdsIncludingInheritedAsync(Guid tenantId, Guid subjectId, CancellationToken cancellationToken)
    {
        var directRoleIds = await dbContext.RoleAssignments.AsNoTracking().Where(assignment => assignment.TenantId == tenantId && assignment.SubjectId == subjectId && assignment.RevokedAtUtc == null).Select(assignment => assignment.RoleId).ToArrayAsync(cancellationToken);
        var allRoles = await dbContext.Roles.AsNoTracking().Where(role => role.TenantId == tenantId).Select(role => new { role.Id, role.ParentRoleId }).ToArrayAsync(cancellationToken);
        var result = new HashSet<Guid>(directRoleIds);
        var frontier = new Queue<Guid>(directRoleIds);
        while (frontier.Count > 0)
        {
            var roleId = frontier.Dequeue();
            var parent = allRoles.FirstOrDefault(role => role.Id == roleId)?.ParentRoleId;
            if (parent is Guid parentId && result.Add(parentId)) frontier.Enqueue(parentId);
        }
        return result.ToArray();
    }

    public async Task<IReadOnlyCollection<string>> GetActiveRoleNamesForSubjectAsync(Guid tenantId, Guid subjectId, CancellationToken cancellationToken) => await dbContext.RoleAssignments.AsNoTracking().Where(assignment => assignment.TenantId == tenantId && assignment.SubjectId == subjectId && assignment.RevokedAtUtc == null).Join(dbContext.Roles.AsNoTracking().Where(role => role.TenantId == tenantId), assignment => assignment.RoleId, role => role.Id, (_, role) => role.Name.Value).Distinct().ToArrayAsync(cancellationToken);
    public Task<RoleAssignment?> GetActiveAsync(Guid tenantId, Guid subjectId, Guid roleId, CancellationToken cancellationToken) => dbContext.RoleAssignments.AsNoTracking().FirstOrDefaultAsync(assignment => assignment.TenantId == tenantId && assignment.SubjectId == subjectId && assignment.RoleId == roleId && assignment.RevokedAtUtc == null, cancellationToken);
    public async Task AddAsync(RoleAssignment assignment, CancellationToken cancellationToken) => await dbContext.RoleAssignments.AddAsync(assignment, cancellationToken);
    public Task UpdateAsync(RoleAssignment assignment, CancellationToken cancellationToken) { dbContext.RoleAssignments.Update(assignment); return Task.CompletedTask; }
}
