namespace AuthorizationService.Domain.Models;

public sealed record AuthorizationDecisionResult(
    bool IsAllowed,
    AuthorizationDecision Decision,
    string ReasonCode,
    string ReasonMessage,
    string? PolicyId,
    string? PolicyVersion,
    DateTimeOffset EvaluatedAtUtc,
    long EvaluationDurationMs,
    Guid CorrelationId)
{
    public static AuthorizationDecisionResult Deny(string reasonCode, string reasonMessage, Guid correlationId, long durationMs = 0, DateTimeOffset? evaluatedAtUtc = null) =>
        new(false, AuthorizationDecision.Deny, reasonCode, reasonMessage, null, null, evaluatedAtUtc ?? DateTimeOffset.UtcNow, durationMs, correlationId);

    public static AuthorizationDecisionResult Allow(string reasonMessage, Guid correlationId, long durationMs, string? policyId, string? policyVersion, DateTimeOffset evaluatedAtUtc) =>
        new(true, AuthorizationDecision.Allow, AuthorizationDecisionReason.Allowed, reasonMessage, policyId, policyVersion, evaluatedAtUtc, durationMs, correlationId);
}
