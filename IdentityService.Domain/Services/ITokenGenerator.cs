using IdentityService.Domain.Aggregates.Session;
using IdentityService.Domain.Aggregates.User;

namespace IdentityService.Domain.Services;

public sealed record GeneratedToken(string AccessToken, DateTimeOffset ExpiresAt);

public interface ITokenGenerator
{
    GeneratedToken GenerateAccessToken(User user, Session session, Guid correlationId);
}
