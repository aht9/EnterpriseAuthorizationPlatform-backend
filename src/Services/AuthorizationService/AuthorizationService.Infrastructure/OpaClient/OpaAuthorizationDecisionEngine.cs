using System.Diagnostics;
using AuthorizationService.Application.Common.Abstractions;
using AuthorizationService.Domain.Models;
using AuthorizationService.Domain.Services;
using Microsoft.Extensions.Logging;

namespace AuthorizationService.Infrastructure.OpaClient;

public sealed class OpaAuthorizationDecisionEngine(OpaHttpClient opaClient, IClock clock, ILogger<OpaAuthorizationDecisionEngine> logger) : IAuthorizationDecisionEngine
{
    public async Task<AuthorizationDecisionResult> EvaluateAsync(AuthorizationDecisionInput input, CancellationToken cancellationToken)
    {
        if (input.TenantId == Guid.Empty) return AuthorizationDecisionResult.Deny(AuthorizationDecisionReason.MissingTenant, "Tenant context is missing.", input.Context.CorrelationId);
        if (!input.IsComplete) return AuthorizationDecisionResult.Deny(AuthorizationDecisionReason.InvalidInput, "Authorization input is incomplete.", input.Context.CorrelationId);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await opaClient.EvaluateAsync(new OpaRequest(ToOpaInput(input)), cancellationToken);
            stopwatch.Stop();
            if (response?.Result is null) return AuthorizationDecisionResult.Deny(AuthorizationDecisionReason.OpaUnavailable, "OPA did not return a decision.", input.Context.CorrelationId, stopwatch.ElapsedMilliseconds, clock.UtcNow);
            return response.Result.Allow
                ? AuthorizationDecisionResult.Allow(response.Result.Reason ?? "Allowed by policy.", input.Context.CorrelationId, stopwatch.ElapsedMilliseconds, response.Result.PolicyId, response.Result.PolicyVersion, clock.UtcNow)
                : AuthorizationDecisionResult.Deny(AuthorizationDecisionReason.DeniedByPolicy, response.Result.Reason ?? "Denied by policy.", input.Context.CorrelationId, stopwatch.ElapsedMilliseconds, clock.UtcNow);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            return AuthorizationDecisionResult.Deny(AuthorizationDecisionReason.OpaTimeout, "OPA evaluation timed out.", input.Context.CorrelationId, stopwatch.ElapsedMilliseconds, clock.UtcNow);
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            logger.LogError(exception, "OPA authorization evaluation failed for tenant {TenantId} correlation {CorrelationId}", input.TenantId, input.Context.CorrelationId);
            return AuthorizationDecisionResult.Deny(AuthorizationDecisionReason.EvaluationError, "Authorization evaluation failed.", input.Context.CorrelationId, stopwatch.ElapsedMilliseconds, clock.UtcNow);
        }
    }

    private static object ToOpaInput(AuthorizationDecisionInput input) => new
    {
        tenant_id = input.TenantId,
        subject = new { id = input.SubjectId, roles = input.Roles, permissions = input.Permissions },
        action = input.Action.Value,
        resource = new { type = input.Resource.ResourceType, id = input.Resource.ResourceId, ownerId = input.Resource.OwnerId, attributes = input.Resource.Attributes },
        context = new { ip_address = input.Context.IpAddress, user_agent = input.Context.UserAgent, environment = input.Context.Environment, usage = input.Context.Usage, correlation_id = input.Context.CorrelationId, request_time = input.Context.RequestTime },
        effective_permissions = input.Permissions
    };
}
