using FluentValidation;

namespace IdentityService.Application.Features.MfaVerify;

public sealed class MfaVerifyCommandValidator : AbstractValidator<MfaVerifyCommand>
{
    public MfaVerifyCommandValidator()
    {
        RuleFor(command => command.Context.TenantId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Code).NotEmpty();
    }
}
