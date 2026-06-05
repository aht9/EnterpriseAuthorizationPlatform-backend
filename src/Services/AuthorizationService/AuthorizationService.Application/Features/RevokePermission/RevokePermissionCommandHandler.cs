using AuthorizationService.Application.Common.Abstractions;
using AuthorizationService.Domain.Repositories;
using MediatR;
using SharedKernel.Errors;
using SharedKernel.Results;

namespace AuthorizationService.Application.Features.RevokePermission;

public sealed class RevokePermissionCommandHandler(IPermissionRepository permissions, IAuthorizationUnitOfWork unitOfWork, IAuthorizationAuditSink auditSink) : IRequestHandler<RevokePermissionCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RevokePermissionCommand command, CancellationToken cancellationToken)
    {
        var grant = await permissions.GetActiveGrantAsync(command.Context.TenantId, command.RoleId, command.PermissionId, cancellationToken);
        if (grant is null) return Result<Unit>.Failure(GeneralErrors.NotFound);
        grant.Revoke(command.Context.CorrelationId);
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        await permissions.UpdateGrantAsync(grant, cancellationToken);
        await unitOfWork.SaveChangesAsync(grant.DomainEvents, cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        await auditSink.RecordStateChangeAsync("AuthorizationService.permission_revoked", command.Context.TenantId, command.Context.CorrelationId, command.Context.UserId, true, command.PermissionId.ToString(), cancellationToken);
        grant.ClearDomainEvents();
        return Result<Unit>.Success(Unit.Value);
    }
}
