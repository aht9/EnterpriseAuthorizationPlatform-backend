using FluentValidation;

namespace IdentityService.Application.Features.DisableUser;

public sealed class DisableUserCommandValidator : AbstractValidator<DisableUserCommand>
{
    public DisableUserCommandValidator()
    {
        RuleFor(command => command.Context.TenantId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
    }
}
