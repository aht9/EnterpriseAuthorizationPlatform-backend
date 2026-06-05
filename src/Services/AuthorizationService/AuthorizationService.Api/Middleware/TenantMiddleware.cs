using System.Text.Json;
using SharedKernel.Errors;
using SharedKernel.Responses;
using SharedKernel.Results;

namespace AuthorizationService.Api.Middleware;

public sealed class TenantMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Tenant-Id";
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/health")) { await next(context); return; }
        var tenantValue = context.User.FindFirst("tenant_id")?.Value ?? context.Request.Headers[HeaderName].FirstOrDefault();
        if (!Guid.TryParse(tenantValue, out var tenantId) || tenantId == Guid.Empty)
        {
            var correlationId = context.Items["CorrelationId"] is Guid value ? value : Guid.Empty;
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(ApiResponse<Unit>.Fail(new ApiError(GeneralErrors.TenantMissing.Code, GeneralErrors.TenantMissing.Description), correlationId));
            return;
        }
        context.Items["TenantId"] = tenantId;
        await next(context);
    }
}
