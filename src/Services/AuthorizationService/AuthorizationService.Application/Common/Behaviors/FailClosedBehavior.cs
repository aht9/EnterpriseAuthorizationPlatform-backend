using AuthorizationService.Application.Common.Abstractions;
using AuthorizationService.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel.Results;

namespace AuthorizationService.Application.Common.Behaviors;

public sealed class FailClosedBehavior<TRequest, TResponse>(ILogger<FailClosedBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try { return await next(); }
        catch (Exception exception) when (request is IAuthorizationDecisionRequest decisionRequest && typeof(TResponse) == typeof(Result<Features.EvaluateAuthorizationDecision.EvaluateAuthorizationDecisionResponse>))
        {
            logger.LogError(exception, "Fail-closed authorization decision tenant={TenantId} correlation={CorrelationId}", decisionRequest.Context.TenantId, decisionRequest.Context.CorrelationId);
            var response = new Features.EvaluateAuthorizationDecision.EvaluateAuthorizationDecisionResponse(false, AuthorizationDecision.Deny.ToString(), AuthorizationDecisionReason.EvaluationError, "Authorization evaluation failed.", null, null, DateTimeOffset.UtcNow, 0, decisionRequest.Context.CorrelationId);
            return (TResponse)(object)Result<Features.EvaluateAuthorizationDecision.EvaluateAuthorizationDecisionResponse>.Success(response);
        }
    }
}
