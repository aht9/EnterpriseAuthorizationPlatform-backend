using SharedKernel.Errors;
using SharedKernel.Responses;

namespace AuthorizationService.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled authorization API exception.");
            var correlationId = context.Items["CorrelationId"] is Guid value ? value : Guid.Empty;
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(new ApiError(GeneralErrors.Failure.Code, "An unexpected error occurred."), correlationId));
        }
    }
}
