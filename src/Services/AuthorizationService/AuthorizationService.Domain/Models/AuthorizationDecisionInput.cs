using AuthorizationService.Domain.ValueObjects;

namespace AuthorizationService.Domain.Models;

public sealed record AuthorizationDecisionInput(
    Guid TenantId,
    Guid SubjectId,
    AuthorizationAction Action,
    ResourceDescriptor Resource,
    AuthorizationContext Context,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions)
{
    public bool IsComplete => TenantId != Guid.Empty && SubjectId != Guid.Empty && Context.CorrelationId != Guid.Empty;
}
