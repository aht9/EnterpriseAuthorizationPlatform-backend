using IdentityService.Application.Common.Abstractions;
using SharedKernel.Contracts.Events;
using SharedKernel.Domain.Events;
using SharedKernel.Infrastructure.Outbox;

namespace IdentityService.Infrastructure.Outbox;

public sealed class InMemoryOutbox : IOutboxRepository
{
    private readonly List<OutboxMessage> messages = [];
    private readonly object sync = new();

    public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        lock (sync) messages.Add(message);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken)
    {
        lock (sync) return Task.FromResult<IReadOnlyCollection<OutboxMessage>>(messages.Where(m => m.ProcessedAt is null).Take(batchSize).ToArray());
    }
}

public sealed class IdentityUnitOfWork(IOutboxRepository outboxRepository) : IIdentityUnitOfWork
{
    public Task BeginTransactionAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task SaveChangesAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            var envelope = new EventEnvelope(domainEvent.EventId, domainEvent.TenantId, domainEvent.CorrelationId, ToEventType(domainEvent), domainEvent.Version, domainEvent.OccurredAt, System.Text.Json.JsonSerializer.Serialize(domainEvent, domainEvent.GetType()));
            await outboxRepository.AddAsync(new OutboxMessage
            {
                Id = envelope.EventId,
                TenantId = envelope.TenantId,
                CorrelationId = envelope.CorrelationId,
                EventType = envelope.EventType,
                Payload = envelope.Payload,
                Version = envelope.Version,
                CreatedAt = envelope.OccurredAt
            }, cancellationToken);
        }
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static string ToEventType(IDomainEvent domainEvent) => domainEvent.GetType().Name switch
    {
        "UserRegisteredDomainEvent" => "identity.user-registered.v1",
        "UserLoggedInDomainEvent" => "identity.user-logged-in.v1",
        "UserDisabledDomainEvent" => "identity.user-disabled.v1",
        "MfaChallengedDomainEvent" => "identity.mfa-challenged.v1",
        "MfaEnabledDomainEvent" => "identity.mfa-enabled.v1",
        "MfaVerifiedDomainEvent" => "identity.mfa-verified.v1",
        "SessionRevokedDomainEvent" => "identity.session-revoked.v1",
        _ => $"identity.{domainEvent.GetType().Name.ToLowerInvariant()}.v1"
    };
}
