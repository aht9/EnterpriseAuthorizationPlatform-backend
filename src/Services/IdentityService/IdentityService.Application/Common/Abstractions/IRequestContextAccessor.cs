namespace IdentityService.Application.Common.Abstractions;

public interface IRequestContextAccessor
{
    RequestContext Current { get; set; }
}
