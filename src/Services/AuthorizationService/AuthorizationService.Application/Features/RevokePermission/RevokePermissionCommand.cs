using AuthorizationService.Application.Common.Abstractions;
using MediatR;
using SharedKernel.Results;

namespace AuthorizationService.Application.Features.RevokePermission;

public sealed record RevokePermissionCommand(Guid RoleId, Guid PermissionId, RequestContext Context) : IRequest<Result<Unit>>;
