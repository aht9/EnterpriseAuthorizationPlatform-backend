namespace AuthorizationService.Application.Common.Abstractions;

public interface IAuthorizationDecisionRequest
{
    RequestContext Context { get; }
}
