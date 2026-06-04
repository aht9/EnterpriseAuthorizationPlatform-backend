using SharedKernel.Responses;
using SharedKernel.Results;

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
            logger.LogError(ex, "Unhandled system exception occurred.");

            var correlationId = context.Items["CorrelationId"] is Guid value
                ? value
                : Guid.Empty;

            var response = ApiResponse<Unit>.Fail(
                new ApiError("Server.InternalError", "An unexpected error occurred."),
                correlationId);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
