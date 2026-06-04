using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IdentityService.Domain.Aggregates.Session;
using IdentityService.Domain.Aggregates.User;
using IdentityService.Domain.Services;
using Microsoft.Extensions.Options;

namespace IdentityService.Infrastructure.Crypto;

public sealed class JwtOptions
{
    public string Issuer { get; init; } = "enterprise-auth-platform";
    public string Audience { get; init; } = "enterprise-services";
    public string SigningKey { get; init; } = "development-signing-key-change-in-secret-store-minimum-32-bytes";
    public int AccessTokenMinutes { get; init; } = 15;
}

public sealed class HmacJwtTokenGenerator(IOptions<JwtOptions> options) : ITokenGenerator
{
    public GeneratedToken GenerateAccessToken(User user, Session session, Guid correlationId)
    {
        var jwtOptions = options.Value;
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(jwtOptions.AccessTokenMinutes);
        var header = new Dictionary<string, object> { ["alg"] = "HS256", ["typ"] = "JWT" };
        var payload = new Dictionary<string, object>
        {
            ["iss"] = jwtOptions.Issuer,
            ["aud"] = jwtOptions.Audience,
            ["sub"] = user.Id.ToString(),
            ["tenant_id"] = user.TenantId.ToString(),
            ["email"] = user.Email.Value,
            ["session_id"] = session.Id.ToString(),
            ["correlation_id"] = correlationId.ToString(),
            ["iat"] = now.ToUnixTimeSeconds(),
            ["nbf"] = now.ToUnixTimeSeconds(),
            ["exp"] = expiresAt.ToUnixTimeSeconds(),
            ["jti"] = Guid.NewGuid().ToString()
        };
        var unsignedToken = $"{Base64Url(JsonSerializer.SerializeToUtf8Bytes(header))}.{Base64Url(JsonSerializer.SerializeToUtf8Bytes(payload))}";
        var signature = HMACSHA256.HashData(Encoding.UTF8.GetBytes(jwtOptions.SigningKey), Encoding.UTF8.GetBytes(unsignedToken));
        return new GeneratedToken($"{unsignedToken}.{Base64Url(signature)}", expiresAt);
    }

    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
