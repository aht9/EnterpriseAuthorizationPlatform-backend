using IdentityService.Application.Common.Abstractions;

namespace IdentityService.Application.Features.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken, RequestContext Context);
