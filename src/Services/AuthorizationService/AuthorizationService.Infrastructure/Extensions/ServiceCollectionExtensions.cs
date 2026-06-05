using AuthorizationService.Application.Common.Abstractions;
using AuthorizationService.Domain.Repositories;
using AuthorizationService.Domain.Services;
using AuthorizationService.Infrastructure.Audit;
using AuthorizationService.Infrastructure.Caching;
using AuthorizationService.Infrastructure.Messaging.Publishers;
using AuthorizationService.Infrastructure.OpaClient;
using AuthorizationService.Infrastructure.Outbox;
using AuthorizationService.Infrastructure.Persistence;
using AuthorizationService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Infrastructure.Messaging;

namespace AuthorizationService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthorizationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OpaOptions>().BindConfiguration("Opa").ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<AuthorizationCacheOptions>().BindConfiguration("AuthorizationCache");
        services.AddOptions<OutboxOptions>().BindConfiguration("Outbox");
        services.AddMemoryCache();
        services.AddDbContextPool<AuthorizationDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("Authorization")));
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRoleAssignmentRepository, RoleAssignmentRepository>();
        services.AddScoped<IAuthorizationUnitOfWork, AuthorizationUnitOfWork>();
        services.AddSingleton<IAuthorizationCache, RedisAuthorizationCache>();
        services.AddScoped<IEffectivePermissionResolver, EffectivePermissionResolver>();
        services.AddSingleton<IAuthorizationAuditSink, LoggingAuthorizationAuditSink>();
        services.AddSingleton<IMessagePublisher, AuthorizationEventPublisher>();
        services.AddHttpClient<OpaHttpClient>((provider, client) => client.BaseAddress = new Uri(provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpaOptions>>().Value.BaseUrl));
        services.AddScoped<IAuthorizationDecisionEngine, OpaAuthorizationDecisionEngine>();
        services.AddHostedService<OutboxProcessor>();
        return services;
    }
}
