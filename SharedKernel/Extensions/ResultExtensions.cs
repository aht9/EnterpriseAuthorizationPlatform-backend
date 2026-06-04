using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Errors;
using SharedKernel.Responses;
using SharedKernel.Results;

namespace SharedKernel.Extensions;

public static class ResultExtensions
{
    public static ApiResponse<T> ToApiResponse<T>(this Result<T> result, Guid correlationId)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            return ApiResponse<T>.Ok(result.Value!, correlationId);
        }

        var validationErrors = result.ValidationErrors.Count > 0
            ? result.ValidationErrors
            : null;

        var apiError = new ApiError(
            result.Error.Code,
            result.Error.Description,
            validationErrors);

        return ApiResponse<T>.Fail(apiError, correlationId);
    }

    public static int ToHttpStatusCode(this ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Failure => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };

    public static IActionResult ToActionResult<T>(
        this Result<T> result,
        ControllerBase controller,
        Guid correlationId)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var response = result.ToApiResponse(correlationId);

        if (result.IsSuccess)
        {
            return controller.Ok(response);
        }

        return controller.StatusCode(result.Error.Type.ToHttpStatusCode(), response);
    }
}
