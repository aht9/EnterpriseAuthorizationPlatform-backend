namespace AuthorizationService.Application.Common.Abstractions;

public interface IRequestContext
{
    RequestContext Current { get; }
}
