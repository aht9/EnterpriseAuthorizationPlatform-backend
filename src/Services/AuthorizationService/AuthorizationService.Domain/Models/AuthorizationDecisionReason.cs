namespace AuthorizationService.Domain.Models;

public static class AuthorizationDecisionReason
{
    public const string Allowed = "Allowed";
    public const string DeniedByPolicy = "DeniedByPolicy";
    public const string OpaUnavailable = "OpaUnavailable";
    public const string OpaTimeout = "OpaTimeout";
    public const string InvalidInput = "InvalidInput";
    public const string MissingTenant = "MissingTenant";
    public const string EvaluationError = "EvaluationError";
}
