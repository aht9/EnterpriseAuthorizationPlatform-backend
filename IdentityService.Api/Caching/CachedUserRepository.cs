using IdentityService.Domain.Aggregates.User;
using IdentityService.Domain.Repositories;
using IdentityService.Domain.ValueObjects;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityService.Api.Caching;

public sealed class CachedUserRepository(IUserRepository inner, IMemoryCache cache) : IUserRepository
{
    private static readonly TimeSpan PositiveCacheTtl = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan NegativeCacheTtl = TimeSpan.FromSeconds(2);

    public async Task<User?> GetByIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken)
    {
        var key = UserByIdKey(tenantId, userId);
        if (cache.TryGetValue(key, out User? cached))
        {
            return cached;
        }

        var user = await inner.GetByIdAsync(tenantId, userId, cancellationToken);
        cache.Set(key, user, user is null ? NegativeCacheTtl : PositiveCacheTtl);
        return user;
    }

    public async Task<User?> GetByEmailAsync(Guid tenantId, Email email, CancellationToken cancellationToken)
    {
        var key = UserByEmailKey(tenantId, email);
        if (cache.TryGetValue(key, out User? cached))
        {
            return cached;
        }

        var user = await inner.GetByEmailAsync(tenantId, email, cancellationToken);
        cache.Set(key, user, user is null ? NegativeCacheTtl : PositiveCacheTtl);
        return user;
    }

    public async Task<bool> ExistsByEmailAsync(Guid tenantId, Email email, CancellationToken cancellationToken)
    {
        var key = UserExistsByEmailKey(tenantId, email);
        if (cache.TryGetValue(key, out bool cached))
        {
            return cached;
        }

        var exists = await inner.ExistsByEmailAsync(tenantId, email, cancellationToken);
        cache.Set(key, exists, exists ? PositiveCacheTtl : NegativeCacheTtl);
        return exists;
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await inner.AddAsync(user, cancellationToken);
        SetUser(user);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        await inner.UpdateAsync(user, cancellationToken);
        SetUser(user);
    }

    private void SetUser(User user)
    {
        cache.Set(UserByIdKey(user.TenantId, user.Id), user, PositiveCacheTtl);
        cache.Set(UserByEmailKey(user.TenantId, user.Email), user, PositiveCacheTtl);
        cache.Set(UserExistsByEmailKey(user.TenantId, user.Email), true, PositiveCacheTtl);
    }

    private static string UserByIdKey(Guid tenantId, Guid userId) => $"identity:user:id:{tenantId:N}:{userId:N}";

    private static string UserByEmailKey(Guid tenantId, Email email) => $"identity:user:email:{tenantId:N}:{email.Value}";

    private static string UserExistsByEmailKey(Guid tenantId, Email email) => $"identity:user:exists-email:{tenantId:N}:{email.Value}";
}
