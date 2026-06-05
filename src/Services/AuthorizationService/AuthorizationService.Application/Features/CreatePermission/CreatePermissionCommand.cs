using AuthorizationService.Application.Common.Abstractions;
using MediatR;
using SharedKernel.Results;

namespace AuthorizationService.Application.Features.CreatePermission;

public sealed record CreatePermissionCommand(string Key, string Action, string ResourceType, string? Description, RequestContext Context) : IRequest<Result<CreatePermissionResponse>>;
