using FluentValidation;

namespace IdentityService.Application.Features.Logout;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(command => command.Context.TenantId).NotEmpty();
        RuleFor(command => command.SessionId).NotEmpty();
    }
}
