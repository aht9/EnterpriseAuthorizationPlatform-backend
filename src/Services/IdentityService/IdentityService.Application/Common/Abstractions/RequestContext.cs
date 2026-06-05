namespace IdentityService.Application.Common.Abstractions;

public sealed record RequestContext(Guid TenantId, Guid CorrelationId, Guid RequestId, string? IpAddress = null, string? UserAgent = null, Guid? UserId = null);
