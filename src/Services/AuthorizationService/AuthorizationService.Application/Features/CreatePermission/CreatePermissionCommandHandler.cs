using AuthorizationService.Application.Common.Abstractions;
using AuthorizationService.Domain.Aggregates.Permission;
using AuthorizationService.Domain.Repositories;
using AuthorizationService.Domain.ValueObjects;
using MediatR;
using SharedKernel.Errors;
using SharedKernel.Results;

namespace AuthorizationService.Application.Features.CreatePermission;

public sealed class CreatePermissionCommandHandler(IPermissionRepository permissions, IAuthorizationUnitOfWork unitOfWork, IAuthorizationAuditSink auditSink) : IRequestHandler<CreatePermissionCommand, Result<CreatePermissionResponse>>
{
    public async Task<Result<CreatePermissionResponse>> Handle(CreatePermissionCommand command, CancellationToken cancellationToken)
    {
        var key = PermissionKey.Create(command.Key);
        if (await permissions.ExistsByKeyAsync(command.Context.TenantId, key, cancellationToken)) return Result<CreatePermissionResponse>.Failure(GeneralErrors.Conflict);
        var permission = Permission.Create(command.Context.TenantId, key, AuthorizationAction.Create(command.Action), command.ResourceType, command.Description, command.Context.CorrelationId);
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        await permissions.AddAsync(permission, cancellationToken);
        await unitOfWork.SaveChangesAsync(permission.DomainEvents, cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        await auditSink.RecordStateChangeAsync("AuthorizationService.permission_created", command.Context.TenantId, command.Context.CorrelationId, command.Context.UserId, true, permission.Key.Value, cancellationToken);
        permission.ClearDomainEvents();
        return Result<CreatePermissionResponse>.Success(new CreatePermissionResponse(permission.Id, permission.Key.Value, permission.Action.Value, permission.ResourceType));
    }
}
