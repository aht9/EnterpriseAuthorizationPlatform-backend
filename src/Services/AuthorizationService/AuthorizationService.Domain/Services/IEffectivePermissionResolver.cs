namespace AuthorizationService.Domain.Services;

public interface IEffectivePermissionResolver
{
    Task<IReadOnlyCollection<string>> ResolveAsync(Guid tenantId, Guid subjectId, CancellationToken cancellationToken);
}
