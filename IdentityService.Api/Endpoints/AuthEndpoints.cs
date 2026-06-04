using IdentityService.Api.Contracts.Requests;
using IdentityService.Api.Extensions;
using IdentityService.Api.Filters;
using IdentityService.Application.Common.Abstractions;
using IdentityService.Api.Contracts.Responses;
using IdentityService.Application.Features.Login;
using IdentityService.Application.Features.Register;
using IdentityService.Application.Features.RefreshToken;
using SharedKernel.Contracts.Api;
using SharedKernel.Responses;

namespace IdentityService.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/identity/auth").WithTags("Identity Auth");

        group.MapPost("/register", async (RegisterRequest request, RegisterCommandHandler handler, IAuditSink auditSink, HttpContext http, CancellationToken ct) =>
        {
            var context = EndpointContext.From(http);
            var result = await handler.HandleAsync(new RegisterCommand(request.Email, request.Password, context), ct);
            await auditSink.RecordAsync("IdentityService.user_registered", context.TenantId, context.CorrelationId, result.Value?.UserId, result.IsSuccess, result.IsFailure ? result.Error.Description : null, ct);
            return result.IsSuccess
                ? Results.Created($"/api/v1/identity/users/{result.Value!.UserId}", ApiResponse<RegisterResponse>.Ok(result.Value!, context.CorrelationId))
                : result.ToHttpResult(context.CorrelationId);
        }).AddEndpointFilter<ValidationFilter<RegisterRequest>>();

        group.MapPost("/login", async (LoginRequest request, LoginCommandHandler handler, IAuditSink auditSink, HttpContext http, CancellationToken ct) =>
        {
            var context = EndpointContext.From(http);
            var result = await handler.HandleAsync(new LoginCommand(request.Email, request.Password, request.MfaCode, context), ct);
            var auditEvent = result.IsSuccess && result.Value!.MfaRequired ? "IdentityService.mfa_required" : result.IsSuccess ? "IdentityService.login_succeeded" : "IdentityService.login_failed";
            await auditSink.RecordAsync(auditEvent, context.TenantId, context.CorrelationId, null, result.IsSuccess, result.IsFailure ? result.Error.Description : null, ct);
            if (result.IsFailure)
            {
                return result.ToHttpResult(context.CorrelationId);
            }

            var response = result.Value!;
            var token = new TokenResponse(response.AccessToken, response.RefreshToken, response.AccessTokenExpiresAt, response.SessionId, response.MfaRequired);
            return Results.Ok(ApiResponse<TokenResponse>.Ok(token, context.CorrelationId));
        }).AddEndpointFilter<ValidationFilter<LoginRequest>>();

        group.MapPost("/refresh", async (RefreshTokenRequest request, RefreshTokenCommandHandler handler, IAuditSink auditSink, HttpContext http, CancellationToken ct) =>
        {
            var context = EndpointContext.From(http);
            var result = await handler.HandleAsync(new RefreshTokenCommand(request.RefreshToken, context), ct);
            await auditSink.RecordAsync(result.IsSuccess ? "IdentityService.refresh_succeeded" : "IdentityService.refresh_failed", context.TenantId, context.CorrelationId, null, result.IsSuccess, result.IsFailure ? result.Error.Description : null, ct);
            if (result.IsFailure)
            {
                return result.ToHttpResult(context.CorrelationId);
            }

            var response = result.Value!;
            var token = new TokenResponse(response.AccessToken, response.RefreshToken, response.AccessTokenExpiresAt, response.SessionId, false);
            return Results.Ok(ApiResponse<TokenResponse>.Ok(token, context.CorrelationId));
        }).AddEndpointFilter<ValidationFilter<RefreshTokenRequest>>();

        return app;
    }
}
