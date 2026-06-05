using AuthorizationService.Application.Features.EvaluateAuthorizationDecision;
using AuthorizationService.Domain.Models;
using MediatR;
using SharedKernel.Results;

namespace AuthorizationService.Application.Features.EvaluateBatchAuthorizationDecisions;

public sealed class EvaluateBatchAuthorizationDecisionsCommandHandler(ISender sender) : IRequestHandler<EvaluateBatchAuthorizationDecisionsCommand, Result<EvaluateBatchAuthorizationDecisionsResponse>>
{
    public async Task<Result<EvaluateBatchAuthorizationDecisionsResponse>> Handle(EvaluateBatchAuthorizationDecisionsCommand command, CancellationToken cancellationToken)
    {
        if (command.Context.TenantId == Guid.Empty)
        {
            return Result<EvaluateBatchAuthorizationDecisionsResponse>.Success(new EvaluateBatchAuthorizationDecisionsResponse(command.Decisions.Select(item => new EvaluateAuthorizationDecisionResponse(false, AuthorizationDecision.Deny.ToString(), AuthorizationDecisionReason.MissingTenant, "Tenant context is missing.", null, null, DateTimeOffset.UtcNow, 0, command.Context.CorrelationId)).ToArray()));
        }

        var results = new List<EvaluateAuthorizationDecisionResponse>(command.Decisions.Count);
        foreach (var decision in command.Decisions)
        {
            try
            {
                var result = await sender.Send(decision with { Context = command.Context }, cancellationToken);
                results.Add(result.IsSuccess ? result.Value! : new EvaluateAuthorizationDecisionResponse(false, AuthorizationDecision.Deny.ToString(), AuthorizationDecisionReason.InvalidInput, result.Error.Description, null, null, DateTimeOffset.UtcNow, 0, command.Context.CorrelationId));
            }
            catch
            {
                results.Add(new EvaluateAuthorizationDecisionResponse(false, AuthorizationDecision.Deny.ToString(), AuthorizationDecisionReason.EvaluationError, "Authorization evaluation failed.", null, null, DateTimeOffset.UtcNow, 0, command.Context.CorrelationId));
            }
        }
        return Result<EvaluateBatchAuthorizationDecisionsResponse>.Success(new EvaluateBatchAuthorizationDecisionsResponse(results));
    }
}
