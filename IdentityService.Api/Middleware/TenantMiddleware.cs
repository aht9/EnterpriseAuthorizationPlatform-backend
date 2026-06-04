namespace IdentityService.Api.Middleware;

public sealed class TenantMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Tenant-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await next(context);
            return;
        }

        var tenantClaim = context.User.FindFirst("tenant_id")?.Value;
        var tenantHeader = context.Request.Headers[HeaderName].FirstOrDefault();
        var tenantValue = tenantClaim ?? tenantHeader;
        if (!Guid.TryParse(tenantValue, out var tenantId) || tenantId == Guid.Empty)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = "TenantId is required. Use JWT tenant_id or X-Tenant-Id." });
            return;
        }
        context.Items["TenantId"] = tenantId;
        await next(context);
    }
}
