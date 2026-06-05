using FluentValidation;

namespace AuthorizationService.Application.Features.CreateRole;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(command => command.Context.TenantId).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(128);
        RuleFor(command => command.Description).MaximumLength(512);
    }
}
