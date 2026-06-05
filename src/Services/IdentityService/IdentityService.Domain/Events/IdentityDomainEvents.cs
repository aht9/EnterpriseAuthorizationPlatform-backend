using SharedKernel.Domain.Events;

namespace IdentityService.Domain.Events;

public abstract record IdentityDomainEvent(Guid TenantId, Guid CorrelationId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
    public int Version { get; } = 1;
}

public sealed record UserRegisteredDomainEvent(Guid UserId, Guid TenantIdValue, string Email, Guid CorrelationIdValue)
    : IdentityDomainEvent(TenantIdValue, CorrelationIdValue);

public sealed record UserLoggedInDomainEvent(Guid UserId, Guid TenantIdValue, Guid SessionId, Guid CorrelationIdValue)
    : IdentityDomainEvent(TenantIdValue, CorrelationIdValue);

public sealed record UserDisabledDomainEvent(Guid UserId, Guid TenantIdValue, Guid CorrelationIdValue)
    : IdentityDomainEvent(TenantIdValue, CorrelationIdValue);

public sealed record MfaChallengedDomainEvent(Guid UserId, Guid TenantIdValue, Guid CorrelationIdValue)
    : IdentityDomainEvent(TenantIdValue, CorrelationIdValue);

public sealed record MfaEnabledDomainEvent(Guid UserId, Guid TenantIdValue, Guid CorrelationIdValue)
    : IdentityDomainEvent(TenantIdValue, CorrelationIdValue);

public sealed record MfaVerifiedDomainEvent(Guid UserId, Guid TenantIdValue, Guid CorrelationIdValue)
    : IdentityDomainEvent(TenantIdValue, CorrelationIdValue);

public sealed record SessionRevokedDomainEvent(Guid SessionId, Guid UserId, Guid TenantIdValue, Guid CorrelationIdValue)
    : IdentityDomainEvent(TenantIdValue, CorrelationIdValue);
