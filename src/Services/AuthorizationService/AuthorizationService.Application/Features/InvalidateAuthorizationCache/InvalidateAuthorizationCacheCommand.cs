using AuthorizationService.Application.Common.Abstractions;
using MediatR;
using SharedKernel.Results;

namespace AuthorizationService.Application.Features.InvalidateAuthorizationCache;

public sealed record InvalidateAuthorizationCacheCommand(Guid SubjectId, RequestContext Context) : IRequest<Result<Unit>>;
