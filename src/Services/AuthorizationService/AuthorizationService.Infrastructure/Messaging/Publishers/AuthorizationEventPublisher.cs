using Microsoft.Extensions.Logging;
using SharedKernel.Contracts.Events;
using SharedKernel.Infrastructure.Messaging;

namespace AuthorizationService.Infrastructure.Messaging.Publishers;

public sealed class AuthorizationEventPublisher(ILogger<AuthorizationEventPublisher> logger) : IMessagePublisher
{
    public Task PublishAsync(EventEnvelope envelope, CancellationToken cancellationToken)
    {
        logger.LogInformation("Published authorization event {EventType} tenant={TenantId} correlation={CorrelationId}", envelope.EventType, envelope.TenantId, envelope.CorrelationId);
        return Task.CompletedTask;
    }
}
