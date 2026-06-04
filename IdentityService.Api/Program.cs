using IdentityService.Api.Endpoints;
using IdentityService.Api.Extensions;
using IdentityService.Api.Middleware;
using IdentityService.Api.Contracts.Responses;
using SharedKernel.Responses;

namespace IdentityService.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddProblemDetails();
        builder.Services.AddIdentityService(builder.Configuration);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
        
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<TenantMiddleware>();
        
        app.MapGet("/health", static () => TypedResults.Ok(ApiResponse<CommandStatusResponse>.Ok(new CommandStatusResponse(), Guid.Empty)));
        app.MapAuthEndpoints();
        app.MapSessionEndpoints();

        app.Run();
    }
}
