using Microsoft.Extensions.Logging;
using SharedKernel.Contracts.Events;
using SharedKernel.Infrastructure.Messaging;

namespace IdentityService.Infrastructure.Messaging.Publishers;

public sealed class UserEventPublisher(ILogger<UserEventPublisher> logger) : IMessagePublisher
{
    public Task PublishAsync(EventEnvelope envelope, CancellationToken cancellationToken)
    {
        logger.LogInformation("Published identity event {EventType} for tenant {TenantId} with correlation {CorrelationId}", envelope.EventType, envelope.TenantId, envelope.CorrelationId);
        return Task.CompletedTask;
    }
}
