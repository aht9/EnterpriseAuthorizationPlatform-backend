using IdentityService.Application.Common.Abstractions;

namespace IdentityService.Application.Features.EnableMfa;

public sealed record EnableMfaCommand(Guid UserId, string Secret, RequestContext Context);
