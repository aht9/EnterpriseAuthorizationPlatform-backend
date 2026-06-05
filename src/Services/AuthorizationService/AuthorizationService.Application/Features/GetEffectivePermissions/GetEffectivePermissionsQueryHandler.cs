using MediatR;
using AuthorizationService.Domain.Services;
using SharedKernel.Results;

namespace AuthorizationService.Application.Features.GetEffectivePermissions;

public sealed class GetEffectivePermissionsQueryHandler(IEffectivePermissionResolver resolver) : IRequestHandler<GetEffectivePermissionsQuery, Result<GetEffectivePermissionsResponse>>
{
    public async Task<Result<GetEffectivePermissionsResponse>> Handle(GetEffectivePermissionsQuery query, CancellationToken cancellationToken)
    {
        var permissions = await resolver.ResolveAsync(query.Context.TenantId, query.SubjectId, cancellationToken);
        return Result<GetEffectivePermissionsResponse>.Success(new GetEffectivePermissionsResponse(query.SubjectId, permissions));
    }
}
