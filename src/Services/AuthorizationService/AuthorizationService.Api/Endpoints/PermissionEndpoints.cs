using AuthorizationService.Api.Contracts.Requests;
using AuthorizationService.Api.Extensions;
using AuthorizationService.Application.Features.GrantPermission;
using SharedKernel.Responses;

namespace AuthorizationService.Api.Endpoints;

public static class PermissionEndpoints
{
    public static IEndpointRouteBuilder MapPermissionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/authorization/permissions").WithTags("Authorization Permissions");
        group.MapPost("/grants", GrantAsync);
        return app;
    }

    private static async ValueTask<IResult> GrantAsync(GrantPermissionRequest request, GrantPermissionCommandHandler handler, HttpContext http, CancellationToken cancellationToken)
    {
        var context = EndpointContext.From(http);
        var result = await handler.HandleAsync(new GrantPermissionCommand(request.RoleId, request.PermissionId, request.GrantedBy, context), cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(ApiResponse<GrantPermissionResponse>.Ok(result.Value!, context.CorrelationId)) : result.ToHttpResult(context.CorrelationId);
    }
}
