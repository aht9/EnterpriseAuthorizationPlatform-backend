using FluentValidation;
using IdentityService.Api.Caching;
using IdentityService.Api.Serialization;
using IdentityService.Api.Validation;
using IdentityService.Application.Common.Abstractions;
using IdentityService.Application.Features.DisableUser;
using IdentityService.Application.Features.EnableMfa;
using IdentityService.Application.Features.Login;
using IdentityService.Application.Features.Logout;
using IdentityService.Application.Features.MfaVerify;
using IdentityService.Application.Features.RefreshToken;
using IdentityService.Application.Features.Register;
using IdentityService.Domain.Repositories;
using IdentityService.Domain.Services;
using IdentityService.Infrastructure.Audit;
using IdentityService.Infrastructure.Crypto;
using IdentityService.Infrastructure.Messaging.Publishers;
using IdentityService.Infrastructure.Mfa;
using IdentityService.Infrastructure.Outbox;
using IdentityService.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Caching.Memory;
using SharedKernel.Infrastructure.Messaging;
using SharedKernel.Infrastructure.Outbox;

namespace IdentityService.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .BindConfiguration("Jwt")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, IdentityApiJsonSerializerContext.Default));
        services.AddMemoryCache();
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<RegisterCommandValidator>();
        services.AddSingleton<UserRepository>();
        services.AddSingleton<IUserRepository>(provider =>
            new CachedUserRepository(provider.GetRequiredService<UserRepository>(), provider.GetRequiredService<IMemoryCache>()));
        services.AddSingleton<SessionRepository>();
        services.AddSingleton<ISessionRepository>(provider =>
            new CachedSessionRepository(provider.GetRequiredService<SessionRepository>(), provider.GetRequiredService<IMemoryCache>()));
        services.AddSingleton<IOutboxRepository, InMemoryOutbox>();
        services.AddSingleton<IIdentityUnitOfWork, IdentityUnitOfWork>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenGenerator, HmacJwtTokenGenerator>();
        services.AddSingleton<IMfaProvider, TotpMfaProvider>();
        services.AddSingleton<IMessagePublisher, UserEventPublisher>();
        services.AddSingleton<IAuditSink, LoggingAuditSink>();
        services.AddScoped<RegisterCommandHandler>();
        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<RefreshTokenCommandHandler>();
        services.AddScoped<LogoutCommandHandler>();
        services.AddScoped<DisableUserCommandHandler>();
        services.AddScoped<MfaVerifyCommandHandler>();
        services.AddScoped<EnableMfaCommandHandler>();
        return services;
    }
}
