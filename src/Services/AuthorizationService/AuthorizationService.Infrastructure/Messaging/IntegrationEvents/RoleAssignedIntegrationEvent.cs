namespace AuthorizationService.Infrastructure.Messaging.IntegrationEvents;

public sealed record RoleAssignedIntegrationEvent(Guid EventId, Guid TenantId, Guid CorrelationId, Guid SubjectId, Guid RoleId, DateTimeOffset OccurredAt);
