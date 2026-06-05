using AuthorizationService.Application.Common.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;
using SharedKernel.Domain.Events;

namespace AuthorizationService.Infrastructure.Persistence;

public sealed class AuthorizationUnitOfWork(AuthorizationDbContext dbContext) : IAuthorizationUnitOfWork
{
    private IDbContextTransaction? transaction;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (transaction is not null) return;
        transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents.Where(domainEvent => domainEvent.TenantId != Guid.Empty))
        {
            await dbContext.OutboxMessages.AddAsync(AuthorizationDbContext.ToOutboxMessage(domainEvent), cancellationToken);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        if (transaction is null) return;
        await transaction.CommitAsync(cancellationToken);
        await transaction.DisposeAsync();
        transaction = null;
    }
}
