using AuthorizationService.Application.Common.Abstractions;
using MediatR;
using SharedKernel.Results;

namespace AuthorizationService.Application.Features.GrantPermission;

public sealed record GrantPermissionCommand(Guid RoleId, Guid PermissionId, Guid GrantedBy, RequestContext Context) : IRequest<Result<GrantPermissionResponse>>;
