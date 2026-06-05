namespace AuthorizationService.Api.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";
    public async Task InvokeAsync(HttpContext context)
    {
        var value = context.Request.Headers[HeaderName].FirstOrDefault();
        var correlationId = Guid.TryParse(value, out var parsed) && parsed != Guid.Empty ? parsed : Guid.NewGuid();
        context.Items["CorrelationId"] = correlationId;
        context.Items["RequestId"] = Guid.NewGuid();
        context.Response.Headers[HeaderName] = correlationId.ToString();
        await next(context);
    }
}
