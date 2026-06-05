using IdentityService.Api.Serialization;
using SharedKernel.Errors;
using SharedKernel.Responses;
using SharedKernel.Results;

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
            var correlationId = context.Items["CorrelationId"] is Guid value
                ? value
                : Guid.Empty;

            var response = ApiResponse<Unit>.Fail(
                new ApiError(GeneralErrors.TenantMissing.Code, GeneralErrors.TenantMissing.Description),
                correlationId);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(response, IdentityApiJsonSerializerContext.Default.ApiResponseUnit);
            return;
        }
        context.Items["TenantId"] = tenantId;
        await next(context);
    }
}
