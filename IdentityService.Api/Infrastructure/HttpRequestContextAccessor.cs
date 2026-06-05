using IdentityService.Application.Common.Abstractions;

namespace IdentityService.Api.Infrastructure;

public sealed class HttpRequestContextAccessor(IHttpContextAccessor httpContextAccessor) : IRequestContextAccessor
{
    public RequestContext Current
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext;
            return new RequestContext(
                httpContext?.Items["TenantId"] is Guid tenantId ? tenantId : Guid.Empty,
                httpContext?.Items["CorrelationId"] is Guid correlationId ? correlationId : Guid.Empty,
                httpContext?.Items["RequestId"] is Guid requestId ? requestId : Guid.Empty,
                httpContext?.Connection.RemoteIpAddress?.ToString(),
                httpContext?.Request.Headers.UserAgent.FirstOrDefault(),
                TryUserId(httpContext));
        }
        set { }
    }

    private static Guid? TryUserId(HttpContext? httpContext) =>
        Guid.TryParse(httpContext?.User.FindFirst("sub")?.Value, out var userId) ? userId : null;
}
