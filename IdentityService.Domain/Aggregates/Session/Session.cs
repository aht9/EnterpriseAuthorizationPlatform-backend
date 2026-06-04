using System.Security.Cryptography;
using IdentityService.Domain.Events;
using SharedKernel.Domain.Primitives;

namespace IdentityService.Domain.Aggregates.Session;

public sealed class Session : AggregateRoot
{
    private Session(Guid id, Guid tenantId, Guid userId, string refreshTokenHash, DateTimeOffset expiresAt, string? ipAddress, string? userAgent)
        : base(id, tenantId)
    {
        UserId = userId;
        RefreshTokenHash = refreshTokenHash;
        ExpiresAt = expiresAt;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public Guid UserId { get; private set; }
    public string RefreshTokenHash { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public bool IsActive => !IsDeleted && RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;

    public static (Session Session, string RefreshToken) Create(Guid tenantId, Guid userId, TimeSpan lifetime, string? ipAddress, string? userAgent)
    {
        var (refreshToken, plain) = RefreshToken.Create(DateTimeOffset.UtcNow.Add(lifetime));
        return (new Session(Guid.NewGuid(), tenantId, userId, refreshToken.TokenHash, refreshToken.ExpiresAt, ipAddress, userAgent), plain);
    }

    public bool MatchesRefreshToken(string refreshToken) => CryptographicOperations.FixedTimeEquals(
        Convert.FromHexString(RefreshToken.Hash(refreshToken)),
        Convert.FromHexString(RefreshTokenHash));

    public string RotateRefreshToken(TimeSpan lifetime)
    {
        var (refreshToken, plain) = RefreshToken.Create(DateTimeOffset.UtcNow.Add(lifetime));
        RefreshTokenHash = refreshToken.TokenHash;
        ExpiresAt = refreshToken.ExpiresAt;
        MarkUpdated();
        return plain;
    }

    public void Revoke(Guid correlationId)
    {
        if (RevokedAt is not null) return;
        RevokedAt = DateTimeOffset.UtcNow;
        SoftDelete();
        RaiseDomainEvent(new SessionRevokedDomainEvent(Id, UserId, TenantId, correlationId));
    }
}
