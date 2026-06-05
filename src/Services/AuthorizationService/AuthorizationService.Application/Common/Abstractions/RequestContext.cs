namespace AuthorizationService.Application.Common.Abstractions;

public sealed record RequestContext(Guid TenantId, Guid CorrelationId, Guid RequestId, Guid? UserId, string? IpAddress = null, string? UserAgent = null);
