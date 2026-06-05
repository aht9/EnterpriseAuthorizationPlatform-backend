using IdentityService.Application.Common.Abstractions;
using IdentityService.Domain.Repositories;
using IdentityService.Domain.Services;
using SharedKernel.Errors;
using SharedKernel.Results;

namespace IdentityService.Application.Features.MfaVerify;

public sealed class MfaVerifyCommandHandler(IUserRepository users, IMfaProvider mfaProvider, IIdentityUnitOfWork unitOfWork)
{
    public async Task<Result<Unit>> HandleAsync(MfaVerifyCommand command, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(command.Context.TenantId, command.UserId, cancellationToken);
        if (user is null)
        {
            return Result<Unit>.Failure(GeneralErrors.NotFound);
        }

        if (!user.MfaSettings.IsEnabled || !mfaProvider.VerifyCode(user.MfaSettings.Secret!, command.Code, DateTimeOffset.UtcNow))
        {
            return Result<Unit>.Failure(GeneralErrors.Unauthorized);
        }
        user.VerifyMfa(command.Context.CorrelationId);
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        await users.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(user.DomainEvents, cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        user.ClearDomainEvents();
        return Result<Unit>.Success(Unit.Value);
    }
}
