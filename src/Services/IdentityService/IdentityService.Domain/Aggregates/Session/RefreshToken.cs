using System.Security.Cryptography;

namespace IdentityService.Domain.Aggregates.Session;

public sealed class RefreshToken
{
    private RefreshToken(string tokenHash, DateTimeOffset expiresAt)
    {
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
    }

    public string TokenHash { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;

    public static (RefreshToken RefreshToken, string PlainText) Create(DateTimeOffset expiresAt)
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var plain = Base64UrlEncode(bytes);
        return (new RefreshToken(Hash(plain), expiresAt), plain);
    }

    public bool Matches(string plainText) => CryptographicOperations.FixedTimeEquals(
        Convert.FromHexString(TokenHash), Convert.FromHexString(Hash(plainText)));

    public void RotateTo(string tokenHash, DateTimeOffset expiresAt)
    {
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        RevokedAt = null;
    }

    public void Revoke() => RevokedAt ??= DateTimeOffset.UtcNow;

    public static string Hash(string token) => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));

    private static string Base64UrlEncode(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
