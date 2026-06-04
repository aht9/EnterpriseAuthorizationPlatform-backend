using IdentityService.Application.Common.Abstractions;
using IdentityService.Domain.Repositories;
using IdentityService.Domain.Services;

namespace IdentityService.Application.Features.MfaVerify;

public sealed class MfaVerifyCommandHandler(IUserRepository users, IMfaProvider mfaProvider, IIdentityUnitOfWork unitOfWork, IAuditSink auditSink)
{
    public async Task HandleAsync(MfaVerifyCommand command, CancellationToken cancellationToken)
    {
        MfaVerifyCommandValidator.Validate(command);
        var user = await users.GetByIdAsync(command.Context.TenantId, command.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("User was not found.");
        if (!user.MfaSettings.IsEnabled || !mfaProvider.VerifyCode(user.MfaSettings.Secret!, command.Code, DateTimeOffset.UtcNow))
        {
            await auditSink.RecordAsync("IdentityService.mfa_failed", user.TenantId, command.Context.CorrelationId, user.Id, false, "Invalid MFA code", cancellationToken);
            throw new UnauthorizedAccessException("Invalid MFA code.");
        }
        user.VerifyMfa(command.Context.CorrelationId);
        await users.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(user.DomainEvents, cancellationToken);
        await auditSink.RecordAsync("IdentityService.mfa_verified", user.TenantId, command.Context.CorrelationId, user.Id, true, null, cancellationToken);
        user.ClearDomainEvents();
    }
}
