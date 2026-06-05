using AuthorizationService.Application.Common.Abstractions;
using MediatR;
using SharedKernel.Results;

namespace AuthorizationService.Application.Features.CreateRole;

public sealed record CreateRoleCommand(string Name, string? Description, Guid? ParentRoleId, RequestContext Context) : IRequest<Result<CreateRoleResponse>>;
