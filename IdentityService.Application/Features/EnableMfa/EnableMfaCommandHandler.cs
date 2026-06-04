using IdentityService.Application.Common.Abstractions;
using IdentityService.Domain.Repositories;

namespace IdentityService.Application.Features.EnableMfa;

public sealed class EnableMfaCommandHandler(IUserRepository users, IIdentityUnitOfWork unitOfWork, IAuditSink auditSink)
{
    public async Task HandleAsync(EnableMfaCommand command, CancellationToken cancellationToken)
    {
        EnableMfaCommandValidator.Validate(command);
        var user = await users.GetByIdAsync(command.Context.TenantId, command.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("User was not found.");

        user.EnableMfa(command.Secret, command.Context.CorrelationId);
        await users.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(user.DomainEvents, cancellationToken);
        await auditSink.RecordAsync("IdentityService.mfa_enabled", user.TenantId, command.Context.CorrelationId, user.Id, true, null, cancellationToken);
        user.ClearDomainEvents();
    }
}
