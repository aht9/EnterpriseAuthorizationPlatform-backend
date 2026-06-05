using AuthorizationService.Application.Common.Abstractions;
using MediatR;
using SharedKernel.Results;

namespace AuthorizationService.Application.Features.AssignRole;

public sealed record AssignRoleCommand(Guid SubjectId, Guid RoleId, Guid AssignedBy, RequestContext Context) : IRequest<Result<AssignRoleResponse>>;
