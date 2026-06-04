namespace SharedKernel.Contracts.Events;

public sealed record EventEnvelope(Guid EventId, Guid TenantId, Guid CorrelationId, string EventType, int Version, DateTimeOffset OccurredAt, string Payload);
