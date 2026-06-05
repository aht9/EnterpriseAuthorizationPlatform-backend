using FluentValidation;

namespace AuthorizationService.Application.Features.EvaluateBatchAuthorizationDecisions;

public sealed class EvaluateBatchAuthorizationDecisionsCommandValidator : AbstractValidator<EvaluateBatchAuthorizationDecisionsCommand>
{
    public EvaluateBatchAuthorizationDecisionsCommandValidator()
    {
        RuleFor(command => command.Context.TenantId).NotEmpty();
        RuleFor(command => command.Decisions).NotEmpty().Must(items => items.Count <= 100);
    }
}
