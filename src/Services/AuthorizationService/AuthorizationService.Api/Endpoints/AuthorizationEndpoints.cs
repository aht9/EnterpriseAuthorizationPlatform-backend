using AuthorizationService.Api.Contracts.Requests;
using AuthorizationService.Api.Contracts.Responses;
using AuthorizationService.Application.Features.EvaluateAuthorizationDecision;
using AuthorizationService.Application.Features.EvaluateBatchAuthorizationDecisions;
using AuthorizationService.Application.Features.GetEffectivePermissions;
using SharedKernel.Responses;

namespace AuthorizationService.Api.Endpoints;

public static class AuthorizationEndpoints
{
    public static IEndpointRouteBuilder MapAuthorizationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/authorization/decisions").WithTags("Authorization Decisions");
        group.MapPost("/evaluate", EvaluateAsync);
        group.MapPost("/batch", BatchAsync);
        group.MapGet("/subjects/{subjectId:guid}/effective-permissions", GetEffectivePermissionsAsync);
        return app;
    }

    private static async ValueTask<IResult> EvaluateAsync(EvaluateAuthorizationRequest request, EvaluateAuthorizationDecisionCommandHandler handler, HttpContext http, CancellationToken cancellationToken)
    {
        var context = EndpointContext.From(http);
        var result = await handler.HandleAsync(new EvaluateAuthorizationDecisionCommand(request.SubjectId, request.Action, request.ResourceType, request.ResourceId, request.OwnerId, request.ResourceAttributes ?? new Dictionary<string, string>(), request.EnvironmentAttributes ?? new Dictionary<string, string>(), request.UsageAttributes ?? new Dictionary<string, string>(), context), cancellationToken);
        if (result.IsFailure) return result.ToHttpResult(context.CorrelationId);
        var value = result.Value!;
        return TypedResults.Ok(ApiResponse<AuthorizationDecisionResponse>.Ok(new AuthorizationDecisionResponse(value.IsAllowed, value.Decision, value.ReasonCode, value.ReasonMessage, value.PolicyId, value.PolicyVersion, value.EvaluatedAtUtc, value.EvaluationDurationMs, value.CorrelationId), context.CorrelationId));
    }

    private static async ValueTask<IResult> BatchAsync(BatchEvaluateAuthorizationRequest request, EvaluateBatchAuthorizationDecisionsCommandHandler handler, HttpContext http, CancellationToken cancellationToken)
    {
        var context = EndpointContext.From(http);
        var commands = request.Decisions.Select(item => new EvaluateAuthorizationDecisionCommand(item.SubjectId, item.Action, item.ResourceType, item.ResourceId, item.OwnerId, item.ResourceAttributes ?? new Dictionary<string, string>(), item.EnvironmentAttributes ?? new Dictionary<string, string>(), item.UsageAttributes ?? new Dictionary<string, string>(), context)).ToArray();
        var result = await handler.HandleAsync(new EvaluateBatchAuthorizationDecisionsCommand(commands, context), cancellationToken);
        if (result.IsFailure) return result.ToHttpResult(context.CorrelationId);
        var decisions = result.Value!.Decisions.Select(value => new AuthorizationDecisionResponse(value.IsAllowed, value.Decision, value.ReasonCode, value.ReasonMessage, value.PolicyId, value.PolicyVersion, value.EvaluatedAtUtc, value.EvaluationDurationMs, value.CorrelationId)).ToArray();
        return TypedResults.Ok(ApiResponse<BatchAuthorizationDecisionResponse>.Ok(new BatchAuthorizationDecisionResponse(decisions), context.CorrelationId));
    }

    private static async ValueTask<IResult> GetEffectivePermissionsAsync(Guid subjectId, GetEffectivePermissionsQueryHandler handler, HttpContext http, CancellationToken cancellationToken)
    {
        var context = EndpointContext.From(http);
        var result = await handler.HandleAsync(new GetEffectivePermissionsQuery(subjectId, context), cancellationToken);
        if (result.IsFailure) return result.ToHttpResult(context.CorrelationId);
        return TypedResults.Ok(ApiResponse<EffectivePermissionsResponse>.Ok(new EffectivePermissionsResponse(result.Value!.SubjectId, result.Value.Permissions), context.CorrelationId));
    }
}
