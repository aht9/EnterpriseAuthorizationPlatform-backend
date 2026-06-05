using AuthorizationService.Api.Contracts.Requests;
using FluentValidation;

namespace AuthorizationService.Api.Validation;

public sealed class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(128);
        RuleFor(request => request.Description).MaximumLength(512);
    }
}

public sealed class AssignRoleRequestValidator : AbstractValidator<AssignRoleRequest>
{
    public AssignRoleRequestValidator()
    {
        RuleFor(request => request.SubjectId).NotEmpty();
        RuleFor(request => request.RoleId).NotEmpty();
        RuleFor(request => request.AssignedBy).NotEmpty();
    }
}

public sealed class RevokeRoleRequestValidator : AbstractValidator<RevokeRoleRequest>
{
    public RevokeRoleRequestValidator()
    {
        RuleFor(request => request.SubjectId).NotEmpty();
        RuleFor(request => request.RoleId).NotEmpty();
    }
}
