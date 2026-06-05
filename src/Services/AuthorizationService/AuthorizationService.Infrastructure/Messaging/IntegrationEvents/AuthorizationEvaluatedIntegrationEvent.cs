namespace AuthorizationService.Infrastructure.Messaging.IntegrationEvents;

public sealed record AuthorizationEvaluatedIntegrationEvent(Guid EventId, Guid TenantId, Guid CorrelationId, Guid SubjectId, string Action, string ResourceType, string ResourceId, bool IsAllowed, string ReasonCode, DateTimeOffset OccurredAt);
