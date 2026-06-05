using AuthorizationService.Application.Common.Abstractions;

namespace AuthorizationService.Api.Endpoints;

public static class EndpointContext
{
    public static RequestContext From(HttpContext http)
    {
        var tenantId = http.Items["TenantId"] is Guid tenant ? tenant : Guid.Empty;
        var correlationId = http.Items["CorrelationId"] is Guid correlation ? correlation : Guid.Empty;
        var requestId = http.Items["RequestId"] is Guid request ? request : Guid.NewGuid();
        Guid? userId = Guid.TryParse(http.User.FindFirst("sub")?.Value, out var subject) ? subject : null;
        return new RequestContext(tenantId, correlationId, requestId, userId, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent.FirstOrDefault());
    }
}
