using FluentValidation;

namespace AuthorizationService.Application.Features.InvalidateAuthorizationCache;

public sealed class InvalidateAuthorizationCacheCommandValidator : AbstractValidator<InvalidateAuthorizationCacheCommand>
{
    public InvalidateAuthorizationCacheCommandValidator()
    {
        RuleFor(command => command.Context.TenantId).NotEmpty();
        RuleFor(command => command.SubjectId).NotEmpty();
    }
}
