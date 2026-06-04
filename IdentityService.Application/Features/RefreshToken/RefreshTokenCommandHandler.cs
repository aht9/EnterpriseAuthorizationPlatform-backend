using IdentityService.Application.Common.Abstractions;
using IdentityService.Domain.Repositories;
using IdentityService.Domain.Services;

namespace IdentityService.Application.Features.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IUserRepository users,
    ISessionRepository sessions,
    ITokenGenerator tokenGenerator,
    IIdentityUnitOfWork unitOfWork,
    IAuditSink auditSink)
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(14);

    public async Task<RefreshTokenResponse> HandleAsync(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        if (command.Context.TenantId == Guid.Empty) throw new ArgumentException("TenantId is required.");
        if (string.IsNullOrWhiteSpace(command.RefreshToken)) throw new ArgumentException("Refresh token is required.");

        var session = await sessions.GetByRefreshTokenAsync(command.Context.TenantId, command.RefreshToken, cancellationToken);
        if (session is null || !session.IsActive)
        {
            await auditSink.RecordAsync("IdentityService.refresh_failed", command.Context.TenantId, command.Context.CorrelationId, null, false, "Invalid refresh token", cancellationToken);
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var user = await users.GetByIdAsync(command.Context.TenantId, session.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            await auditSink.RecordAsync("IdentityService.refresh_failed", command.Context.TenantId, command.Context.CorrelationId, session.UserId, false, "Inactive user", cancellationToken);
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var newRefreshToken = session.RotateRefreshToken(RefreshTokenLifetime);
        await sessions.UpdateAsync(session, cancellationToken);
        await unitOfWork.SaveChangesAsync(session.DomainEvents, cancellationToken);
        var accessToken = tokenGenerator.GenerateAccessToken(user, session, command.Context.CorrelationId);
        await auditSink.RecordAsync("IdentityService.refresh_succeeded", session.TenantId, command.Context.CorrelationId, user.Id, true, null, cancellationToken);
        session.ClearDomainEvents();
        return new RefreshTokenResponse(accessToken.AccessToken, newRefreshToken, accessToken.ExpiresAt, session.Id);
    }
}
