using MediatR;
using AuthorizationService.Application.Common.Abstractions;
using AuthorizationService.Domain.Aggregates.PermissionGrant;
using AuthorizationService.Domain.Repositories;
using SharedKernel.Errors;
using SharedKernel.Results;

namespace AuthorizationService.Application.Features.GrantPermission;

public sealed class GrantPermissionCommandHandler(IRoleRepository roles, IPermissionRepository permissions, IAuthorizationUnitOfWork unitOfWork, IAuthorizationAuditSink auditSink) : IRequestHandler<GrantPermissionCommand, Result<GrantPermissionResponse>>
{
    public async Task<Result<GrantPermissionResponse>> Handle(GrantPermissionCommand command, CancellationToken cancellationToken)
    {
        var role = await roles.GetByIdAsync(command.Context.TenantId, command.RoleId, cancellationToken);
        var permission = await permissions.GetByIdAsync(command.Context.TenantId, command.PermissionId, cancellationToken);
        if (role is null || permission is null) return Result<GrantPermissionResponse>.Failure(GeneralErrors.NotFound);
        var existing = await permissions.GetActiveGrantAsync(command.Context.TenantId, command.RoleId, command.PermissionId, cancellationToken);
        if (existing is not null) return Result<GrantPermissionResponse>.Failure(GeneralErrors.Conflict);
        var grant = PermissionGrant.Grant(command.Context.TenantId, command.RoleId, command.PermissionId, command.GrantedBy, command.Context.CorrelationId);
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        await permissions.AddGrantAsync(grant, cancellationToken);
        await unitOfWork.SaveChangesAsync(grant.DomainEvents, cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        await auditSink.RecordStateChangeAsync("AuthorizationService.permission_granted", command.Context.TenantId, command.Context.CorrelationId, command.Context.UserId, true, command.PermissionId.ToString(), cancellationToken);
        grant.ClearDomainEvents();
        return Result<GrantPermissionResponse>.Success(new GrantPermissionResponse(grant.Id, grant.RoleId, grant.PermissionId, grant.GrantedAtUtc));
    }
}
