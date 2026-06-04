using FluentValidation;

namespace IdentityService.Application.Features.RefreshToken;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(command => command.Context.TenantId).NotEmpty();
        RuleFor(command => command.RefreshToken).NotEmpty();
    }
}
