using MediatR;
using AuthorizationService.Application.Common.Abstractions;
using AuthorizationService.Domain.Aggregates.Role;
using AuthorizationService.Domain.Repositories;
using AuthorizationService.Domain.ValueObjects;
using SharedKernel.Errors;
using SharedKernel.Results;

namespace AuthorizationService.Application.Features.CreateRole;

public sealed class CreateRoleCommandHandler(IRoleRepository roles, IAuthorizationUnitOfWork unitOfWork, IAuthorizationAuditSink auditSink) : IRequestHandler<CreateRoleCommand, Result<CreateRoleResponse>>
{
    public async Task<Result<CreateRoleResponse>> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var name = RoleName.Create(command.Name);
        if (await roles.ExistsByNameAsync(command.Context.TenantId, name, cancellationToken)) return Result<CreateRoleResponse>.Failure(GeneralErrors.Conflict);
        var role = Role.Create(command.Context.TenantId, name, command.Description, command.ParentRoleId, command.Context.CorrelationId);
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        await roles.AddAsync(role, cancellationToken);
        await unitOfWork.SaveChangesAsync(role.DomainEvents, cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        await auditSink.RecordStateChangeAsync("AuthorizationService.role_created", command.Context.TenantId, command.Context.CorrelationId, command.Context.UserId, true, role.Name.Value, cancellationToken);
        role.ClearDomainEvents();
        return Result<CreateRoleResponse>.Success(new CreateRoleResponse(role.Id, role.Name.Value, role.Status.ToString()));
    }
}
