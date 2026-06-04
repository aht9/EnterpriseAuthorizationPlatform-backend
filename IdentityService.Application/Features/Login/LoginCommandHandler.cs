using IdentityService.Application.Common.Abstractions;
using IdentityService.Domain.Aggregates.Session;
using IdentityService.Domain.Aggregates.User;
using IdentityService.Domain.Repositories;
using IdentityService.Domain.Services;
using IdentityService.Domain.ValueObjects;

namespace IdentityService.Application.Features.Login;

public sealed class LoginCommandHandler(
    IUserRepository users,
    ISessionRepository sessions,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator,
    IMfaProvider mfaProvider,
    IIdentityUnitOfWork unitOfWork,
    IAuditSink auditSink)
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(14);

    public async Task<LoginResponse> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        LoginCommandValidator.Validate(command);
        var email = Email.Create(command.Email);
        var user = await users.GetByEmailAsync(command.Context.TenantId, email, cancellationToken);
        if (user is null || user.Status == UserStatus.Disabled || !passwordHasher.Verify(command.Password, user.PasswordHash.Value))
        {
            if (user is not null)
            {
                user.RecordFailedLogin(command.Context.CorrelationId);
                await users.UpdateAsync(user, cancellationToken);
                await unitOfWork.SaveChangesAsync(user.DomainEvents, cancellationToken);
                user.ClearDomainEvents();
            }
            await auditSink.RecordAsync("IdentityService.login_failed", command.Context.TenantId, command.Context.CorrelationId, user?.Id, false, "Invalid credentials", cancellationToken);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (user.MfaSettings.IsEnabled)
        {
            if (string.IsNullOrWhiteSpace(command.MfaCode))
            {
                user.ChallengeMfa(command.Context.CorrelationId);
                await users.UpdateAsync(user, cancellationToken);
                await unitOfWork.SaveChangesAsync(user.DomainEvents, cancellationToken);
                await auditSink.RecordAsync("IdentityService.mfa_required", user.TenantId, command.Context.CorrelationId, user.Id, true, null, cancellationToken);
                user.ClearDomainEvents();
                return new LoginResponse(string.Empty, string.Empty, DateTimeOffset.MinValue, Guid.Empty, true);
            }

            if (!mfaProvider.VerifyCode(user.MfaSettings.Secret!, command.MfaCode, DateTimeOffset.UtcNow))
            {
                await auditSink.RecordAsync("IdentityService.mfa_failed", user.TenantId, command.Context.CorrelationId, user.Id, false, "Invalid MFA code", cancellationToken);
                throw new UnauthorizedAccessException("Invalid MFA code.");
            }
            user.VerifyMfa(command.Context.CorrelationId);
        }

        var (session, refreshToken) = Session.Create(user.TenantId, user.Id, RefreshTokenLifetime, command.Context.IpAddress, command.Context.UserAgent);
        user.RecordSuccessfulLogin(session.Id, command.Context.CorrelationId);
        await sessions.AddAsync(session, cancellationToken);
        await users.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(user.DomainEvents.Concat(session.DomainEvents).ToArray(), cancellationToken);
        var accessToken = tokenGenerator.GenerateAccessToken(user, session, command.Context.CorrelationId);
        await auditSink.RecordAsync("IdentityService.login_succeeded", user.TenantId, command.Context.CorrelationId, user.Id, true, null, cancellationToken);
        user.ClearDomainEvents();
        session.ClearDomainEvents();
        return new LoginResponse(accessToken.AccessToken, refreshToken, accessToken.ExpiresAt, session.Id, false);
    }
}
