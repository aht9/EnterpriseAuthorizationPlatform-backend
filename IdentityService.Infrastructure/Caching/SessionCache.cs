namespace IdentityService.Infrastructure.Caching;

public sealed class SessionCache
{
    private readonly Dictionary<string, DateTimeOffset> entries = [];

    public void Set(Guid tenantId, Guid sessionId, DateTimeOffset expiresAt) => entries[Key(tenantId, sessionId)] = expiresAt;
    public bool IsActive(Guid tenantId, Guid sessionId) => entries.TryGetValue(Key(tenantId, sessionId), out var expiresAt) && expiresAt > DateTimeOffset.UtcNow;
    public void Remove(Guid tenantId, Guid sessionId) => entries.Remove(Key(tenantId, sessionId));

    private static string Key(Guid tenantId, Guid sessionId) => $"identity:session:v1:{tenantId}:{sessionId}";
}
