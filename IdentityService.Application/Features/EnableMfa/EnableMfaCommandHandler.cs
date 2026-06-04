using IdentityService.Application.Common.Abstractions;
using IdentityService.Domain.Repositories;
using SharedKernel.Errors;
using SharedKernel.Results;

namespace IdentityService.Application.Features.EnableMfa;

public sealed class EnableMfaCommandHandler(IUserRepository users, IIdentityUnitOfWork unitOfWork)
{
    public async Task<Result<Unit>> HandleAsync(EnableMfaCommand command, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(command.Context.TenantId, command.UserId, cancellationToken);
        if (user is null)
        {
            return Result<Unit>.Failure(GeneralErrors.NotFound);
        }

        user.EnableMfa(command.Secret, command.Context.CorrelationId);
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        await users.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(user.DomainEvents, cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        user.ClearDomainEvents();
        return Result<Unit>.Success(Unit.Value);
    }
}
