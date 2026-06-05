using IdentityService.Application.Common.Abstractions;

namespace IdentityService.Api.Endpoints;

internal static class EndpointContext
{
    public static RequestContext From(HttpContext httpContext) => new(
        (Guid)httpContext.Items["TenantId"]!,
        (Guid)httpContext.Items["CorrelationId"]!,
        (Guid)httpContext.Items["RequestId"]!,
        httpContext.Connection.RemoteIpAddress?.ToString(),
        httpContext.Request.Headers.UserAgent.FirstOrDefault(),
        TryUserId(httpContext));

    private static Guid? TryUserId(HttpContext httpContext) =>
        Guid.TryParse(httpContext.User.FindFirst("sub")?.Value, out var userId) ? userId : null;
}
