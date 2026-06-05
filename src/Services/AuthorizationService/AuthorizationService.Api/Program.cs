using AuthorizationService.Api.Contracts.Responses;
using AuthorizationService.Api.Endpoints;
using AuthorizationService.Api.Extensions;
using AuthorizationService.Api.Middleware;
using SharedKernel.Responses;

namespace AuthorizationService.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddProblemDetails();
        builder.Services.AddAuthorizationService(builder.Configuration);

        var app = builder.Build();

        if (app.Environment.IsDevelopment()) app.MapOpenApi();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<TenantMiddleware>();
        app.MapGet("/health", static () => TypedResults.Ok(ApiResponse<CommandStatusResponse>.Ok(new CommandStatusResponse(), Guid.Empty)));
        app.MapAuthorizationEndpoints();
        app.MapRoleEndpoints();
        app.MapPermissionEndpoints();
        app.Run();
    }
}