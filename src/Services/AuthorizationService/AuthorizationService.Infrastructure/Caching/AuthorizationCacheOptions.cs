namespace AuthorizationService.Infrastructure.Caching;

public sealed class AuthorizationCacheOptions
{
    public bool Enabled { get; init; } = true;
    public int EffectivePermissionsTtlSeconds { get; init; } = 300;
    public bool DecisionCacheEnabled { get; init; }
    public int DecisionCacheTtlSeconds { get; init; } = 30;
}
