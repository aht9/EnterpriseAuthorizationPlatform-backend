using IdentityService.Application.Common.Abstractions;

namespace IdentityService.Application.Features.DisableUser;

public sealed record DisableUserCommand(Guid UserId, RequestContext Context);
