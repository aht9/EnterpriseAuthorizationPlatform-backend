using IdentityService.Api.Contracts.Requests;
using IdentityService.Api.Extensions;
using IdentityService.Api.Filters;
using IdentityService.Application.Common.Abstractions;
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

        group.MapPost("/sessions/logout", async (LogoutRequest request, LogoutCommandHandler handler, IAuditSink auditSink, HttpContext http, CancellationToken ct) =>
        {
            var context = EndpointContext.From(http);
            var result = await handler.HandleAsync(new LogoutCommand(request.SessionId, context), ct);
            await auditSink.RecordAsync("IdentityService.logout", context.TenantId, context.CorrelationId, null, result.IsSuccess, result.IsFailure ? result.Error.Description : null, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse<object>.Ok(new { loggedOut = true }, context.CorrelationId))
                : result.ToHttpResult(context.CorrelationId);
        }).AddEndpointFilter<ValidationFilter<LogoutRequest>>();

        group.MapPost("/mfa/verify", async (MfaVerifyRequest request, MfaVerifyCommandHandler handler, IAuditSink auditSink, HttpContext http, CancellationToken ct) =>
        {
            var context = EndpointContext.From(http);
            var result = await handler.HandleAsync(new MfaVerifyCommand(request.UserId, request.Code, context), ct);
            await auditSink.RecordAsync(result.IsSuccess ? "IdentityService.mfa_verified" : "IdentityService.mfa_failed", context.TenantId, context.CorrelationId, request.UserId, result.IsSuccess, result.IsFailure ? result.Error.Description : null, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse<object>.Ok(new { verified = true }, context.CorrelationId))
                : result.ToHttpResult(context.CorrelationId);
        }).AddEndpointFilter<ValidationFilter<MfaVerifyRequest>>();

        group.MapPost("/mfa/enable", async (EnableMfaRequest request, EnableMfaCommandHandler handler, IAuditSink auditSink, HttpContext http, CancellationToken ct) =>
        {
            var context = EndpointContext.From(http);
            var result = await handler.HandleAsync(new EnableMfaCommand(request.UserId, request.Secret, context), ct);
            await auditSink.RecordAsync("IdentityService.mfa_enabled", context.TenantId, context.CorrelationId, request.UserId, result.IsSuccess, result.IsFailure ? result.Error.Description : null, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse<object>.Ok(new { enabled = true }, context.CorrelationId))
                : result.ToHttpResult(context.CorrelationId);
        }).AddEndpointFilter<ValidationFilter<EnableMfaRequest>>();

        group.MapPost("/users/{userId:guid}/disable", async (Guid userId, DisableUserCommandHandler handler, IAuditSink auditSink, HttpContext http, CancellationToken ct) =>
        {
            var context = EndpointContext.From(http);
            var result = await handler.HandleAsync(new DisableUserCommand(userId, context), ct);
            await auditSink.RecordAsync("identity.user_disabled", context.TenantId, context.CorrelationId, userId, result.IsSuccess, result.IsFailure ? result.Error.Description : null, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse<object>.Ok(new { disabled = true }, context.CorrelationId))
                : result.ToHttpResult(context.CorrelationId);
        });

        return app;
    }
}
