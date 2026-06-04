namespace SharedKernel.Responses;

public sealed record ApiResponse<T>
{
    private ApiResponse(bool success, T? data, ApiError? error, Guid correlationId)
    {
        Success = success;
        Data = data;
        Error = error;
        CorrelationId = correlationId;
    }

    public bool Success { get; }
    public T? Data { get; }
    public ApiError? Error { get; }
    public Guid CorrelationId { get; }

    public static ApiResponse<T> Ok(T data, Guid correlationId) =>
        new(true, data, null, correlationId);

    public static ApiResponse<T> Fail(ApiError error, Guid correlationId)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new ApiResponse<T>(false, default, error, correlationId);
    }
}
