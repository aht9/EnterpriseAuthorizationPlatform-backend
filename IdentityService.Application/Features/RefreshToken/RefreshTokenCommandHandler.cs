using IdentityService.Application.Common.Abstractions;
using IdentityService.Domain.Repositories;
using IdentityService.Domain.Services;
using SharedKernel.Errors;
using SharedKernel.Results;

namespace IdentityService.Application.Features.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IUserRepository users,
    ISessionRepository sessions,
    ITokenGenerator tokenGenerator,
    IIdentityUnitOfWork unitOfWork)
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(14);

    public async Task<Result<RefreshTokenResponse>> HandleAsync(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var session = await sessions.GetByRefreshTokenAsync(command.Context.TenantId, command.RefreshToken, cancellationToken);
        if (session is null || !session.IsActive)
        {
            return Result<RefreshTokenResponse>.Failure(GeneralErrors.Unauthorized);
        }

        var user = await users.GetByIdAsync(command.Context.TenantId, session.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Result<RefreshTokenResponse>.Failure(GeneralErrors.Unauthorized);
        }

        var newRefreshToken = session.RotateRefreshToken(RefreshTokenLifetime);
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        await sessions.UpdateAsync(session, cancellationToken);
        await unitOfWork.SaveChangesAsync(session.DomainEvents, cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        var accessToken = tokenGenerator.GenerateAccessToken(user, session, command.Context.CorrelationId);
        session.ClearDomainEvents();
        return Result<RefreshTokenResponse>.Success(new RefreshTokenResponse(accessToken.AccessToken, newRefreshToken, accessToken.ExpiresAt, session.Id));
    }
}
