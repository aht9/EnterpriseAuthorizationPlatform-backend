using MediatR;
using AuthorizationService.Application.Common.Abstractions;
using AuthorizationService.Domain.Aggregates.RoleAssignment;
using AuthorizationService.Domain.Repositories;
using SharedKernel.Errors;
using SharedKernel.Results;

namespace AuthorizationService.Application.Features.AssignRole;

public sealed class AssignRoleCommandHandler(IRoleRepository roles, IRoleAssignmentRepository assignments, IAuthorizationUnitOfWork unitOfWork, IAuthorizationAuditSink auditSink, IAuthorizationCache cache) : IRequestHandler<AssignRoleCommand, Result<AssignRoleResponse>>
{
    public async Task<Result<AssignRoleResponse>> Handle(AssignRoleCommand command, CancellationToken cancellationToken)
    {
        var role = await roles.GetByIdAsync(command.Context.TenantId, command.RoleId, cancellationToken);
        if (role is null) return Result<AssignRoleResponse>.Failure(GeneralErrors.NotFound);
        var existing = await assignments.GetActiveAsync(command.Context.TenantId, command.SubjectId, command.RoleId, cancellationToken);
        if (existing is not null) return Result<AssignRoleResponse>.Failure(GeneralErrors.Conflict);
        var assignment = RoleAssignment.Assign(command.Context.TenantId, command.SubjectId, command.RoleId, command.AssignedBy, command.Context.CorrelationId);
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        await assignments.AddAsync(assignment, cancellationToken);
        await unitOfWork.SaveChangesAsync(assignment.DomainEvents, cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        await cache.InvalidateSubjectAsync(command.Context.TenantId, command.SubjectId, cancellationToken);
        await auditSink.RecordStateChangeAsync("AuthorizationService.role_assigned", command.Context.TenantId, command.Context.CorrelationId, command.SubjectId, true, command.RoleId.ToString(), cancellationToken);
        assignment.ClearDomainEvents();
        return Result<AssignRoleResponse>.Success(new AssignRoleResponse(assignment.Id, assignment.SubjectId, assignment.RoleId, assignment.AssignedAtUtc));
    }
}
