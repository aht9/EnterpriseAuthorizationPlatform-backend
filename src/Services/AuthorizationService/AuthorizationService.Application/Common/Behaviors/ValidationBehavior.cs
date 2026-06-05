using FluentValidation;
using MediatR;
using SharedKernel.Errors;
using SharedKernel.Results;

namespace AuthorizationService.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var failures = validators.SelectMany(validator => validator.Validate(request).Errors).Where(failure => failure is not null).Select(failure => new ValidationError(failure.PropertyName, failure.ErrorMessage)).ToArray();
        if (failures.Length == 0) return await next();

        var responseType = typeof(TResponse);
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GetGenericArguments()[0];
            var method = typeof(Result<>).MakeGenericType(valueType).GetMethod(nameof(Result<object>.ValidationFailure), [typeof(IReadOnlyCollection<ValidationError>)]);
            return (TResponse)method!.Invoke(null, [failures])!;
        }

        throw new ValidationException(failures.Select(error => new FluentValidation.Results.ValidationFailure(error.PropertyName, error.Message)));
    }
}
