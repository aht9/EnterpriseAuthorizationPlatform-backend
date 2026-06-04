using IdentityService.Application.Common.Abstractions;

namespace IdentityService.Application.Features.MfaVerify;

public sealed record MfaVerifyCommand(Guid UserId, string Code, RequestContext Context);
