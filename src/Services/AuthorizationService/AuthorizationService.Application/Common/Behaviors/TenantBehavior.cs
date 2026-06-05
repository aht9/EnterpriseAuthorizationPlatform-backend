using AuthorizationService.Application.Common.Abstractions;
using AuthorizationService.Application.Common.Exceptions;
using AuthorizationService.Domain.Models;
using MediatR;
using SharedKernel.Results;

namespace AuthorizationService.Application.Common.Behaviors;

public sealed class TenantBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IAuthorizationDecisionRequest decisionRequest && decisionRequest.Context.TenantId == Guid.Empty)
        {
            var response = new Features.EvaluateAuthorizationDecision.EvaluateAuthorizationDecisionResponse(false, AuthorizationDecision.Deny.ToString(), AuthorizationDecisionReason.MissingTenant, "Tenant context is missing.", null, null, DateTimeOffset.UtcNow, 0, decisionRequest.Context.CorrelationId);
            return (TResponse)(object)Result<Features.EvaluateAuthorizationDecision.EvaluateAuthorizationDecisionResponse>.Success(response);
        }

        var context = request.GetType().GetProperty("Context")?.GetValue(request) as RequestContext;
        if (context is not null && context.TenantId == Guid.Empty) throw new TenantMissingException();
        return await next();
    }
}
