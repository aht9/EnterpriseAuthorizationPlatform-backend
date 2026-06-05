using IdentityService.Application.Common.Abstractions;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SharedKernel.Domain.Events;
using SharedKernel.Infrastructure.Outbox;

namespace IdentityService.Infrastructure.Outbox;

public sealed class OutboxRepository(IdentityDbContext dbContext) : IOutboxRepository
{
    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken) =>
        await dbContext.OutboxMessages.AddAsync(message, cancellationToken);

    public async Task<IReadOnlyCollection<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken) =>
        await dbContext.OutboxMessages
            .AsNoTracking()
            .Where(message => message.ProcessedAt is null && (message.NextRetryAt == null || message.NextRetryAt <= DateTimeOffset.UtcNow))
            .OrderBy(message => message.CreatedAt)
            .Take(batchSize)
            .ToArrayAsync(cancellationToken);
}

public sealed class IdentityUnitOfWork(IdentityDbContext dbContext) : IIdentityUnitOfWork
{
    private IDbContextTransaction? transaction;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (transaction is not null)
        {
            return;
        }

        transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        if (transaction is null)
        {
            return;
        }

        await transaction.CommitAsync(cancellationToken);
        await transaction.DisposeAsync();
        transaction = null;
    }
}
