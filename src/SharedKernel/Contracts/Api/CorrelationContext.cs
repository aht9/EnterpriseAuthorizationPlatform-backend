namespace SharedKernel.Contracts.Api;

public sealed record CorrelationContext(Guid TenantId, Guid CorrelationId, Guid RequestId, Guid? UserId);
