using IdentityService.Api.Endpoints;
using IdentityService.Api.Extensions;
using IdentityService.Api.Middleware;

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
        
        app.MapGet("/health", () => Results.Ok(new { service = "identity", status = "healthy" }));
        app.MapAuthEndpoints();
        app.MapSessionEndpoints();

        app.Run();
    }
}