using SharedKernel.Domain.Events;

namespace IdentityService.Application.Common.Abstractions;

public interface IIdentityUnitOfWork
{
    Task BeginTransactionAsync(CancellationToken cancellationToken);
    Task SaveChangesAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken);
    Task CommitTransactionAsync(CancellationToken cancellationToken);
}
