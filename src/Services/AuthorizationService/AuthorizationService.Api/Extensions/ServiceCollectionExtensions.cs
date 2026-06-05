using AuthorizationService.Api.Infrastructure;
using AuthorizationService.Api.Validation;
using AuthorizationService.Application.Common.Abstractions;
using AuthorizationService.Application.Features.AssignRole;
using AuthorizationService.Application.Features.CreateRole;
using AuthorizationService.Application.Features.EvaluateAuthorizationDecision;
using AuthorizationService.Application.Features.EvaluateBatchAuthorizationDecisions;
using AuthorizationService.Application.Features.GetEffectivePermissions;
using AuthorizationService.Application.Features.GrantPermission;
using AuthorizationService.Application.Features.InvalidateAuthorizationCache;
using AuthorizationService.Application.Features.RevokeRole;
using AuthorizationService.Infrastructure.Extensions;
using FluentValidation;

namespace AuthorizationService.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthorizationService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IRequestContext, HttpRequestContext>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddValidatorsFromAssemblyContaining<EvaluateAuthorizationRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EvaluateAuthorizationDecisionCommandValidator>();
        services.AddAuthorizationInfrastructure(configuration);
        services.AddScoped<EvaluateAuthorizationDecisionCommandHandler>();
        services.AddScoped<EvaluateBatchAuthorizationDecisionsCommandHandler>();
        services.AddScoped<GetEffectivePermissionsQueryHandler>();
        services.AddScoped<CreateRoleCommandHandler>();
        services.AddScoped<AssignRoleCommandHandler>();
        services.AddScoped<GrantPermissionCommandHandler>();
        services.AddScoped<RevokeRoleCommandHandler>();
        services.AddScoped<InvalidateAuthorizationCacheCommandHandler>();
        return services;
    }
}

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
