namespace IdentityService.Api.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveGuid(context.Request.Headers[HeaderName].FirstOrDefault()) ?? Guid.NewGuid();
        var requestId = ResolveGuid(context.Request.Headers["X-Request-Id"].FirstOrDefault()) ?? Guid.NewGuid();
        context.Items["CorrelationId"] = correlationId;
        context.Items["RequestId"] = requestId;
        context.Response.Headers[HeaderName] = correlationId.ToString();
        context.Response.Headers["X-Request-Id"] = requestId.ToString();
        await next(context);
    }

    private static Guid? ResolveGuid(string? value) => Guid.TryParse(value, out var id) ? id : null;
}
