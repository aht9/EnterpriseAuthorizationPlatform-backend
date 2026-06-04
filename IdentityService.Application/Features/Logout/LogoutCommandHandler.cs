using IdentityService.Application.Common.Abstractions;
using IdentityService.Domain.Repositories;
using SharedKernel.Results;

namespace IdentityService.Application.Features.Logout;

public sealed class LogoutCommandHandler(ISessionRepository sessions, IIdentityUnitOfWork unitOfWork)
{
    public async Task<Result<Unit>> HandleAsync(LogoutCommand command, CancellationToken cancellationToken)
    {
        var session = await sessions.GetByIdAsync(command.Context.TenantId, command.SessionId, cancellationToken);
        if (session is null)
        {
            return Result<Unit>.Success(Unit.Value);
        }

        session.Revoke(command.Context.CorrelationId);
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        await sessions.UpdateAsync(session, cancellationToken);
        await unitOfWork.SaveChangesAsync(session.DomainEvents, cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        session.ClearDomainEvents();
        return Result<Unit>.Success(Unit.Value);
    }
}
