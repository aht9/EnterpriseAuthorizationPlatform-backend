using IdentityService.Api.Contracts.Requests;
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

        group.MapPost("/register", async (RegisterRequest request, RegisterCommandHandler handler, HttpContext http, CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(new RegisterCommand(request.Email, request.Password, EndpointContext.From(http)), ct);
            return Results.Created($"/api/v1/identity/users/{response.UserId}", ApiResponse<RegisterResponse>.Ok(response, (Guid)http.Items["CorrelationId"]!));
        });

        group.MapPost("/login", async (LoginRequest request, LoginCommandHandler handler, HttpContext http, CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(new LoginCommand(request.Email, request.Password, request.MfaCode, EndpointContext.From(http)), ct);
            var token = new TokenResponse(response.AccessToken, response.RefreshToken, response.AccessTokenExpiresAt, response.SessionId, response.MfaRequired);
            return Results.Ok(ApiResponse<TokenResponse>.Ok(token, (Guid)http.Items["CorrelationId"]!));
        });

        group.MapPost("/refresh", async (RefreshTokenRequest request, RefreshTokenCommandHandler handler, HttpContext http, CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(new RefreshTokenCommand(request.RefreshToken, EndpointContext.From(http)), ct);
            var token = new TokenResponse(response.AccessToken, response.RefreshToken, response.AccessTokenExpiresAt, response.SessionId, false);
            return Results.Ok(ApiResponse<TokenResponse>.Ok(token, (Guid)http.Items["CorrelationId"]!));
        });

        return app;
    }
}
