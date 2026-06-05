using MediatR;
using AuthorizationService.Application.Common.Abstractions;
using AuthorizationService.Domain.Repositories;
using SharedKernel.Errors;
using SharedKernel.Results;

namespace AuthorizationService.Application.Features.RevokeRole;

public sealed class RevokeRoleCommandHandler(IRoleAssignmentRepository assignments, IAuthorizationUnitOfWork unitOfWork, IAuthorizationAuditSink auditSink, IAuthorizationCache cache) : IRequestHandler<RevokeRoleCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RevokeRoleCommand command, CancellationToken cancellationToken)
    {
        var assignment = await assignments.GetActiveAsync(command.Context.TenantId, command.SubjectId, command.RoleId, cancellationToken);
        if (assignment is null) return Result<Unit>.Failure(GeneralErrors.NotFound);
        assignment.Revoke(command.Context.CorrelationId);
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        await assignments.UpdateAsync(assignment, cancellationToken);
        await unitOfWork.SaveChangesAsync(assignment.DomainEvents, cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        await cache.InvalidateSubjectAsync(command.Context.TenantId, command.SubjectId, cancellationToken);
        await auditSink.RecordStateChangeAsync("AuthorizationService.role_revoked", command.Context.TenantId, command.Context.CorrelationId, command.SubjectId, true, command.RoleId.ToString(), cancellationToken);
        assignment.ClearDomainEvents();
        return Result<Unit>.Success(Unit.Value);
    }
}
