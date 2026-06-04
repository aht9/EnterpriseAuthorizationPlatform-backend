using IdentityService.Api.Contracts.Requests;
using IdentityService.Api.Contracts.Responses;
using IdentityService.Api.Extensions;
using IdentityService.Api.Filters;
using IdentityService.Application.Common.Abstractions;
using IdentityService.Application.Features.DisableUser;
using IdentityService.Application.Features.EnableMfa;
using IdentityService.Application.Features.Logout;
using IdentityService.Application.Features.MfaVerify;
using SharedKernel.Contracts.Api;

namespace IdentityService.Api.Endpoints;

public static class SessionEndpoints
{
    public static IEndpointRouteBuilder MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/identity").WithTags("Identity Sessions");

        group.MapPost("/sessions/logout", LogoutAsync)
            .AddEndpointFilter<ValidationFilter<LogoutRequest>>();

        group.MapPost("/mfa/verify", VerifyMfaAsync)
            .AddEndpointFilter<ValidationFilter<MfaVerifyRequest>>();

        group.MapPost("/mfa/enable", EnableMfaAsync)
            .AddEndpointFilter<ValidationFilter<EnableMfaRequest>>();

        group.MapPost("/users/{userId:guid}/disable", DisableUserAsync);

        return app;
    }

    private static async ValueTask<IResult> LogoutAsync(
        LogoutRequest request,
        LogoutCommandHandler handler,
        IAuditSink auditSink,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var context = EndpointContext.From(http);
        var result = await handler.HandleAsync(new LogoutCommand(request.SessionId, context), cancellationToken);
        await auditSink.RecordAsync("IdentityService.logout", context.TenantId, context.CorrelationId, null, result.IsSuccess, result.IsFailure ? result.Error.Description : null, cancellationToken);
        return result.IsSuccess
            ? ResultHttpExtensions.ToOkHttpResult(new CommandStatusResponse(LoggedOut: true), context.CorrelationId)
            : result.ToHttpResult(context.CorrelationId);
    }

    private static async ValueTask<IResult> VerifyMfaAsync(
        MfaVerifyRequest request,
        MfaVerifyCommandHandler handler,
        IAuditSink auditSink,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var context = EndpointContext.From(http);
        var result = await handler.HandleAsync(new MfaVerifyCommand(request.UserId, request.Code, context), cancellationToken);
        await auditSink.RecordAsync(result.IsSuccess ? "IdentityService.mfa_verified" : "IdentityService.mfa_failed", context.TenantId, context.CorrelationId, request.UserId, result.IsSuccess, result.IsFailure ? result.Error.Description : null, cancellationToken);
        return result.IsSuccess
            ? ResultHttpExtensions.ToOkHttpResult(new CommandStatusResponse(Verified: true), context.CorrelationId)
            : result.ToHttpResult(context.CorrelationId);
    }

    private static async ValueTask<IResult> EnableMfaAsync(
        EnableMfaRequest request,
        EnableMfaCommandHandler handler,
        IAuditSink auditSink,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var context = EndpointContext.From(http);
        var result = await handler.HandleAsync(new EnableMfaCommand(request.UserId, request.Secret, context), cancellationToken);
        await auditSink.RecordAsync("IdentityService.mfa_enabled", context.TenantId, context.CorrelationId, request.UserId, result.IsSuccess, result.IsFailure ? result.Error.Description : null, cancellationToken);
        return result.IsSuccess
            ? ResultHttpExtensions.ToOkHttpResult(new CommandStatusResponse(Enabled: true), context.CorrelationId)
            : result.ToHttpResult(context.CorrelationId);
    }

    private static async ValueTask<IResult> DisableUserAsync(
        Guid userId,
        DisableUserCommandHandler handler,
        IAuditSink auditSink,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var context = EndpointContext.From(http);
        var result = await handler.HandleAsync(new DisableUserCommand(userId, context), cancellationToken);
        await auditSink.RecordAsync("identity.user_disabled", context.TenantId, context.CorrelationId, userId, result.IsSuccess, result.IsFailure ? result.Error.Description : null, cancellationToken);
        return result.IsSuccess
            ? ResultHttpExtensions.ToOkHttpResult(new CommandStatusResponse(Disabled: true), context.CorrelationId)
            : result.ToHttpResult(context.CorrelationId);
    }
}
