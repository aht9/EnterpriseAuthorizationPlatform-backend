namespace AuthorizationService.Infrastructure.Outbox;

public sealed class OutboxOptions
{
    public int BatchSize { get; init; } = 50;
    public int PollingIntervalSeconds { get; init; } = 5;
}
