using SharedKernel.Domain.Events;

namespace IdentityService.Application.Common.Abstractions;

public interface IIdentityUnitOfWork
{
    Task SaveChangesAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken);
}
