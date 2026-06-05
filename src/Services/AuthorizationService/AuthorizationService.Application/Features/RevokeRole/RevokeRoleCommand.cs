using AuthorizationService.Application.Common.Abstractions;
using MediatR;
using SharedKernel.Results;

namespace AuthorizationService.Application.Features.RevokeRole;

public sealed record RevokeRoleCommand(Guid SubjectId, Guid RoleId, RequestContext Context) : IRequest<Result<Unit>>;
