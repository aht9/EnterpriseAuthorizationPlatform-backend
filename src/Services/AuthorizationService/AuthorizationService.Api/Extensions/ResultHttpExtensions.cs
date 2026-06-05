using SharedKernel.Errors;
using SharedKernel.Extensions;

namespace AuthorizationService.Api.Extensions;

public static class ResultHttpExtensions
{
    public static IResult ToHttpResult<T>(this SharedKernel.Results.Result<T> result, Guid correlationId) => result.Error.Type switch
    {
        ErrorType.Validation => TypedResults.BadRequest(result.ToApiResponse(correlationId)),
        ErrorType.Unauthorized => TypedResults.Json(result.ToApiResponse(correlationId), statusCode: StatusCodes.Status401Unauthorized),
        ErrorType.Forbidden => TypedResults.Problem(statusCode: StatusCodes.Status403Forbidden),
        ErrorType.NotFound => TypedResults.NotFound(result.ToApiResponse(correlationId)),
        ErrorType.Conflict => TypedResults.Conflict(result.ToApiResponse(correlationId)),
        _ when result.IsSuccess => TypedResults.Ok(result.ToApiResponse(correlationId)),
        _ => TypedResults.Problem(title: "Request failed.", detail: result.Error.Description, statusCode: StatusCodes.Status500InternalServerError)
    };
}
