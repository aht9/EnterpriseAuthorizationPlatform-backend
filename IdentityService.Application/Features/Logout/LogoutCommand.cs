using IdentityService.Application.Common.Abstractions;

namespace IdentityService.Application.Features.Logout;

public sealed record LogoutCommand(Guid SessionId, RequestContext Context);
