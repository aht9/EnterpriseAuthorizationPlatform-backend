using FluentValidation;

namespace AuthorizationService.Application.Features.AssignRole;

public sealed class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(command => command.Context.TenantId).NotEmpty();
        RuleFor(command => command.SubjectId).NotEmpty();
        RuleFor(command => command.RoleId).NotEmpty();
        RuleFor(command => command.AssignedBy).NotEmpty();
    }
}
