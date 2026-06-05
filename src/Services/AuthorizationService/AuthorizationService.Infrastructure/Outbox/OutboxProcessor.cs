using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Contracts.Events;
using SharedKernel.Infrastructure.Messaging;

namespace AuthorizationService.Infrastructure.Outbox;

public sealed class OutboxProcessor(IServiceScopeFactory scopeFactory, IOptions<OutboxOptions> options, ILogger<OutboxProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessAsync(stoppingToken); }
            catch (Exception exception) { logger.LogError(exception, "Authorization outbox processing failed."); }
            await Task.Delay(TimeSpan.FromSeconds(options.Value.PollingIntervalSeconds), stoppingToken);
        }
    }

    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Persistence.AuthorizationDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();
        var messages = await dbContext.OutboxMessages.Where(message => message.ProcessedAt == null && (message.NextRetryAt == null || message.NextRetryAt <= DateTimeOffset.UtcNow)).OrderBy(message => message.CreatedAt).Take(options.Value.BatchSize).ToArrayAsync(cancellationToken);
        foreach (var message in messages)
        {
            try
            {
                await publisher.PublishAsync(new EventEnvelope(message.Id, message.TenantId, message.CorrelationId, message.EventType, message.Version, message.CreatedAt, message.Payload), cancellationToken);
                message.MarkProcessed();
            }
            catch (Exception exception)
            {
                message.MarkFailed(exception.Message);
            }
        }
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
