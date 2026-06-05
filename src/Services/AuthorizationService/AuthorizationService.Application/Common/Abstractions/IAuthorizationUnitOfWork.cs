using SharedKernel.Domain.Events;

namespace AuthorizationService.Application.Common.Abstractions;

public interface IAuthorizationUnitOfWork
{
    Task BeginTransactionAsync(CancellationToken cancellationToken);
    Task SaveChangesAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken);
    Task CommitTransactionAsync(CancellationToken cancellationToken);
}
