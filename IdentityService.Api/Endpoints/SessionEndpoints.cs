using IdentityService.Api.Contracts.Requests;
using IdentityService.Application.Features.DisableUser;
using IdentityService.Application.Features.EnableMfa;
using IdentityService.Application.Features.Logout;
using IdentityService.Application.Features.MfaVerify;
using SharedKernel.Contracts.Api;
using SharedKernel.Responses;

namespace IdentityService.Api.Endpoints;

public static class SessionEndpoints
{
    public static IEndpointRouteBuilder MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/identity").WithTags("Identity Sessions");

        group.MapPost("/sessions/logout", async (LogoutRequest request, LogoutCommandHandler handler, HttpContext http, CancellationToken ct) =>
        {
            await handler.HandleAsync(new LogoutCommand(request.SessionId, EndpointContext.From(http)), ct);
            return Results.Ok(ApiResponse<object>.Ok(new { loggedOut = true }, (Guid)http.Items["CorrelationId"]!));
        });

        group.MapPost("/mfa/verify", async (MfaVerifyRequest request, MfaVerifyCommandHandler handler, HttpContext http, CancellationToken ct) =>
        {
            await handler.HandleAsync(new MfaVerifyCommand(request.UserId, request.Code, EndpointContext.From(http)), ct);
            return Results.Ok(ApiResponse<object>.Ok(new { verified = true }, (Guid)http.Items["CorrelationId"]!));
        });

        group.MapPost("/mfa/enable", async (EnableMfaRequest request, EnableMfaCommandHandler handler, HttpContext http, CancellationToken ct) =>
        {
            await handler.HandleAsync(new EnableMfaCommand(request.UserId, request.Secret, EndpointContext.From(http)), ct);
            return Results.Ok(ApiResponse<object>.Ok(new { enabled = true }, (Guid)http.Items["CorrelationId"]!));
        });

        group.MapPost("/users/{userId:guid}/disable", async (Guid userId, DisableUserCommandHandler handler, HttpContext http, CancellationToken ct) =>
        {
            await handler.HandleAsync(new DisableUserCommand(userId, EndpointContext.From(http)), ct);
            return Results.Ok(ApiResponse<object>.Ok(new { disabled = true }, (Guid)http.Items["CorrelationId"]!));
        });

        return app;
    }
}
