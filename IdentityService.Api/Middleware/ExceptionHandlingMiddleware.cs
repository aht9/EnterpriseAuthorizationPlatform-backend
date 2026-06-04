
using SharedKernel.Errors;

namespace IdentityService.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var (status, error) = ex switch
            {
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, Error.Unauthorized),
                ArgumentException => (StatusCodes.Status400BadRequest, new Error("General.Validation", ex.Message)),
                InvalidOperationException => (StatusCodes.Status409Conflict, new Error("General.Conflict", ex.Message)),
                KeyNotFoundException => (StatusCodes.Status404NotFound, Error.NotFound),
                _ => (StatusCodes.Status500InternalServerError, new Error("General.Unexpected", "An unexpected error occurred."))
            };
            if (status == StatusCodes.Status500InternalServerError) logger.LogError(ex, "Unhandled identity API exception");
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new { success = false, error, correlationId = context.Items["CorrelationId"] });
        }
    }
}
