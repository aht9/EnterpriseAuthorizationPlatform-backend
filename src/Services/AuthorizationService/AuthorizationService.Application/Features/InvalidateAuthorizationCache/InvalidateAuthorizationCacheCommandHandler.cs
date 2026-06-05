using MediatR;
using AuthorizationService.Application.Common.Abstractions;
using SharedKernel.Results;

namespace AuthorizationService.Application.Features.InvalidateAuthorizationCache;

public sealed class InvalidateAuthorizationCacheCommandHandler(IAuthorizationCache cache, IAuthorizationAuditSink auditSink) : IRequestHandler<InvalidateAuthorizationCacheCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(InvalidateAuthorizationCacheCommand command, CancellationToken cancellationToken)
    {
        await cache.InvalidateSubjectAsync(command.Context.TenantId, command.SubjectId, cancellationToken);
        await auditSink.RecordStateChangeAsync("AuthorizationService.cache_invalidated", command.Context.TenantId, command.Context.CorrelationId, command.SubjectId, true, null, cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}
