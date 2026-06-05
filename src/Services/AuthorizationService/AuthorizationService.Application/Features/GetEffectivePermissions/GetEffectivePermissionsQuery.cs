using AuthorizationService.Application.Common.Abstractions;
using MediatR;
using SharedKernel.Results;

namespace AuthorizationService.Application.Features.GetEffectivePermissions;

public sealed record GetEffectivePermissionsQuery(Guid SubjectId, RequestContext Context) : IRequest<Result<GetEffectivePermissionsResponse>>;
