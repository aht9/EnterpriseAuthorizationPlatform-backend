using IdentityService.Application.Common.Abstractions;
using IdentityService.Domain.Aggregates.Session;
using IdentityService.Domain.Aggregates.User;
using IdentityService.Domain.Repositories;
using IdentityService.Domain.Services;
using IdentityService.Domain.ValueObjects;
using SharedKernel.Errors;
using SharedKernel.Results;

namespace IdentityService.Application.Features.Login;

public sealed class LoginCommandHandler(
    IUserRepository users,
    ISessionRepository sessions,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator,
    IMfaProvider mfaProvider,
    IIdentityUnitOfWork unitOfWork)
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(14);

    public async Task<Result<LoginResponse>> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
    {
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
            return Result<LoginResponse>.Failure(GeneralErrors.Unauthorized);
        }

        if (user.MfaSettings.IsEnabled)
        {
            if (string.IsNullOrWhiteSpace(command.MfaCode))
            {
                user.ChallengeMfa(command.Context.CorrelationId);
                await users.UpdateAsync(user, cancellationToken);
                await unitOfWork.SaveChangesAsync(user.DomainEvents, cancellationToken);
                user.ClearDomainEvents();
                return Result<LoginResponse>.Success(new LoginResponse(string.Empty, string.Empty, DateTimeOffset.MinValue, Guid.Empty, true));
            }

            if (!mfaProvider.VerifyCode(user.MfaSettings.Secret!, command.MfaCode, DateTimeOffset.UtcNow))
            {
                return Result<LoginResponse>.Failure(GeneralErrors.Unauthorized);
            }
            user.VerifyMfa(command.Context.CorrelationId);
        }

        var (session, refreshToken) = Session.Create(user.TenantId, user.Id, RefreshTokenLifetime, command.Context.IpAddress, command.Context.UserAgent);
        user.RecordSuccessfulLogin(session.Id, command.Context.CorrelationId);
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        await sessions.AddAsync(session, cancellationToken);
        await users.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(user.DomainEvents.Concat(session.DomainEvents).ToArray(), cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        var accessToken = tokenGenerator.GenerateAccessToken(user, session, command.Context.CorrelationId);
        user.ClearDomainEvents();
        session.ClearDomainEvents();
        return Result<LoginResponse>.Success(new LoginResponse(accessToken.AccessToken, refreshToken, accessToken.ExpiresAt, session.Id, false));
    }
}
