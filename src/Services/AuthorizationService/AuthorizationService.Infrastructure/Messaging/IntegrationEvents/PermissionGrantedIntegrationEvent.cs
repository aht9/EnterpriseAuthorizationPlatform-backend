namespace AuthorizationService.Infrastructure.Messaging.IntegrationEvents;

public sealed record PermissionGrantedIntegrationEvent(Guid EventId, Guid TenantId, Guid CorrelationId, Guid RoleId, Guid PermissionId, DateTimeOffset OccurredAt);
