using AuthorizationService.Api.Contracts.Requests;
using AuthorizationService.Api.Extensions;
using AuthorizationService.Application.Features.AssignRole;
using AuthorizationService.Application.Features.CreateRole;
using AuthorizationService.Application.Features.RevokeRole;
using SharedKernel.Responses;

namespace AuthorizationService.Api.Endpoints;

public static class RoleEndpoints
{
    public static IEndpointRouteBuilder MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/authorization/roles").WithTags("Authorization Roles");
        group.MapPost("/", CreateAsync);
        group.MapPost("/assignments", AssignAsync);
        group.MapPost("/assignments/revoke", RevokeAsync);
        return app;
    }

    private static async ValueTask<IResult> CreateAsync(CreateRoleRequest request, CreateRoleCommandHandler handler, HttpContext http, CancellationToken cancellationToken)
    {
        var context = EndpointContext.From(http);
        var result = await handler.HandleAsync(new CreateRoleCommand(request.Name, request.Description, request.ParentRoleId, context), cancellationToken);
        return result.IsSuccess ? TypedResults.Created($"/api/v1/authorization/roles/{result.Value!.RoleId}", ApiResponse<Application.Features.CreateRole.CreateRoleResponse>.Ok(result.Value!, context.CorrelationId)) : result.ToHttpResult(context.CorrelationId);
    }

    private static async ValueTask<IResult> AssignAsync(AssignRoleRequest request, AssignRoleCommandHandler handler, HttpContext http, CancellationToken cancellationToken)
    {
        var context = EndpointContext.From(http);
        var result = await handler.HandleAsync(new AssignRoleCommand(request.SubjectId, request.RoleId, request.AssignedBy, context), cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(ApiResponse<AssignRoleResponse>.Ok(result.Value!, context.CorrelationId)) : result.ToHttpResult(context.CorrelationId);
    }

    private static async ValueTask<IResult> RevokeAsync(RevokeRoleRequest request, RevokeRoleCommandHandler handler, HttpContext http, CancellationToken cancellationToken)
    {
        var context = EndpointContext.From(http);
        var result = await handler.HandleAsync(new RevokeRoleCommand(request.SubjectId, request.RoleId, context), cancellationToken);
        return result.ToHttpResult(context.CorrelationId);
    }
}
