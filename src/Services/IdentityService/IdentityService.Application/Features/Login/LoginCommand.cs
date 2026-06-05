using IdentityService.Application.Common.Abstractions;

namespace IdentityService.Application.Features.Login;

public sealed record LoginCommand(string Email, string Password, string? MfaCode, RequestContext Context);
