using IdentityService.Application.Common.Abstractions;

namespace IdentityService.Application.Features.Register;

public sealed record RegisterCommand(string Email, string Password, RequestContext Context);
