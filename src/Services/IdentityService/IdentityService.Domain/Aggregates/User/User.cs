using IdentityService.Domain.Events;
using IdentityService.Domain.ValueObjects;
using SharedKernel.Domain.Primitives;

namespace IdentityService.Domain.Aggregates.User;

public sealed class User : AggregateRoot
{
    private User(Guid id, Guid tenantId, Email email, PasswordHash passwordHash) : base(id, tenantId)
    {
        Email = email;
        PasswordHash = passwordHash;
        Status = UserStatus.Active;
    }

    public Email Email { get; private set; }
    public PasswordHash PasswordHash { get; private set; }
    public UserStatus Status { get; private set; }
    public MfaSettings MfaSettings { get; private set; } = MfaSettings.Disabled;
    public int FailedLoginAttempts { get; private set; }

    public bool IsActive => Status == UserStatus.Active || Status == UserStatus.MfaPending;

    public static User Register(Guid tenantId, Email email, PasswordHash passwordHash, Guid correlationId)
    {
        var user = new User(Guid.NewGuid(), tenantId, email, passwordHash);
        user.RaiseDomainEvent(new UserRegisteredDomainEvent(user.Id, tenantId, email.Value, correlationId));
        return user;
    }

    public void Disable(Guid correlationId)
    {
        if (Status == UserStatus.Disabled) return;
        Status = UserStatus.Disabled;
        MarkUpdated();
        RaiseDomainEvent(new UserDisabledDomainEvent(Id, TenantId, correlationId));
    }

    public void RecordFailedLogin(Guid correlationId)
    {
        FailedLoginAttempts++;
        MarkUpdated();
        if (FailedLoginAttempts >= 5) Disable(correlationId);
    }

    public void RecordSuccessfulLogin(Guid sessionId, Guid correlationId)
    {
        FailedLoginAttempts = 0;
        MarkUpdated();
        RaiseDomainEvent(new UserLoggedInDomainEvent(Id, TenantId, sessionId, correlationId));
    }

    public void ChallengeMfa(Guid correlationId)
    {
        Status = UserStatus.MfaPending;
        MarkUpdated();
        RaiseDomainEvent(new MfaChallengedDomainEvent(Id, TenantId, correlationId));
    }

    public void VerifyMfa(Guid correlationId)
    {
        Status = UserStatus.Active;
        MarkUpdated();
        RaiseDomainEvent(new MfaVerifiedDomainEvent(Id, TenantId, correlationId));
    }

    public void EnableMfa(string secret, Guid correlationId)
    {
        MfaSettings = MfaSettings.Enable(secret);
        MarkUpdated();
        RaiseDomainEvent(new MfaEnabledDomainEvent(Id, TenantId, correlationId));
    }
}
