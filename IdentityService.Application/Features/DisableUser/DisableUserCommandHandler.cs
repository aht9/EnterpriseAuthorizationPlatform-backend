using IdentityService.Application.Common.Abstractions;
using IdentityService.Domain.Repositories;

namespace IdentityService.Application.Features.DisableUser;

public sealed class DisableUserCommandHandler(IUserRepository users, IIdentityUnitOfWork unitOfWork, IAuditSink auditSink)
{
    public async Task HandleAsync(DisableUserCommand command, CancellationToken cancellationToken)
    {
        DisableUserCommandValidator.Validate(command);
        var user = await users.GetByIdAsync(command.Context.TenantId, command.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("User was not found.");
        user.Disable(command.Context.CorrelationId);
        await users.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(user.DomainEvents, cancellationToken);
        await auditSink.RecordAsync("identity.user_disabled", user.TenantId, command.Context.CorrelationId, user.Id, true, null, cancellationToken);
        user.ClearDomainEvents();
    }
}
