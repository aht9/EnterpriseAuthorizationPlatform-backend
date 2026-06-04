using SharedKernel.Errors;
using SharedKernel.Extensions;
using SharedKernel.Results;

namespace IdentityService.Api.Extensions;

public static class ResultHttpExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, Guid correlationId) =>
        result.Error.Type switch
        {
            ErrorType.Validation => Results.BadRequest(result.ToApiResponse(correlationId)),
            ErrorType.Unauthorized => Results.Unauthorized(),
            ErrorType.Forbidden => Results.Forbid(),
            ErrorType.NotFound => Results.NotFound(result.ToApiResponse(correlationId)),
            ErrorType.Conflict => Results.Conflict(result.ToApiResponse(correlationId)),
            _ when result.IsSuccess => Results.Ok(result.ToApiResponse(correlationId)),
            _ => Results.BadRequest(result.ToApiResponse(correlationId))
        };
}
