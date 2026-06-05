using System.Linq.Expressions;
using System.Text.Json;
using IdentityService.Application.Common.Abstractions;
using IdentityService.Domain.Aggregates.Session;
using IdentityService.Domain.Aggregates.User;
using IdentityService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Contracts.Events;
using SharedKernel.Domain.Events;
using SharedKernel.Domain.Primitives;
using SharedKernel.Infrastructure.Outbox;

namespace IdentityService.Infrastructure.Persistence;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options, IRequestContextAccessor requestContextAccessor)
    : DbContext(options)
{
    private const string Schema = "Identity";

    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.Entity<User>(ConfigureUser);
        modelBuilder.Entity<Session>(ConfigureSession);
        modelBuilder.Entity<OutboxMessage>(ConfigureOutboxMessage);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(entityType => typeof(Entity).IsAssignableFrom(entityType.ClrType)))
        {
            entityType.SetQueryFilter(CreateTenantSoftDeleteFilter(entityType.ClrType));
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddOutboxMessages();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ConfigureUser(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<User> builder)
    {
        builder.ToTable("identity_users");
        builder.HasKey(user => new { user.TenantId, user.Id });
        builder.HasIndex(user => new { user.TenantId, user.Id });
        builder.HasIndex(user => new { user.TenantId, user.Email }).IsUnique();

        builder.Property(user => user.Id).ValueGeneratedNever();
        builder.Property(user => user.TenantId).ValueGeneratedNever();
        builder.Property(user => user.Version).IsConcurrencyToken();
        builder.Property(user => user.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(user => user.FailedLoginAttempts);
        builder.Property(user => user.Email)
            .HasConversion(email => email.Value, value => Email.Create(value))
            .HasMaxLength(320);
        builder.Property(user => user.PasswordHash)
            .HasConversion(passwordHash => passwordHash.Value, value => PasswordHash.FromHash(value))
            .HasMaxLength(512);
        builder.OwnsOne(user => user.MfaSettings, owned =>
        {
            owned.Property(settings => settings.IsEnabled).HasColumnName("MfaEnabled");
            owned.Property(settings => settings.Secret).HasColumnName("MfaSecret").HasMaxLength(256);
        });
        builder.Ignore(user => user.IsActive);
        builder.Ignore(user => user.DomainEvents);
    }

    private void ConfigureSession(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("identity_sessions");
        builder.HasKey(session => new { session.TenantId, session.Id });
        builder.HasIndex(session => new { session.TenantId, session.Id });
        builder.HasIndex(session => new { session.TenantId, session.RefreshTokenHash });

        builder.Property(session => session.Id).ValueGeneratedNever();
        builder.Property(session => session.TenantId).ValueGeneratedNever();
        builder.Property(session => session.UserId);
        builder.Property(session => session.RefreshTokenHash).HasMaxLength(128);
        builder.Property(session => session.ExpiresAt);
        builder.Property(session => session.RevokedAt);
        builder.Property(session => session.IpAddress).HasMaxLength(64);
        builder.Property(session => session.UserAgent).HasMaxLength(512);
        builder.Property(session => session.Version).IsConcurrencyToken();
        builder.Ignore(session => session.IsActive);
        builder.Ignore(session => session.DomainEvents);
    }

    private static void ConfigureOutboxMessage(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("identity_outbox");
        builder.HasKey(message => message.Id);
        builder.HasIndex(message => new { message.TenantId, message.ProcessedAt, message.NextRetryAt });
        builder.Property(message => message.Id).ValueGeneratedNever();
        builder.Property(message => message.EventType).HasMaxLength(256);
        builder.Property(message => message.Payload).HasColumnType("jsonb");
        builder.Property(message => message.Error).HasMaxLength(2048);
    }

    private LambdaExpression CreateTenantSoftDeleteFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "entity");
        var tenantId = Expression.Property(parameter, nameof(Entity.TenantId));
        var isDeleted = Expression.Property(parameter, nameof(Entity.IsDeleted));
        var currentContext = Expression.Property(Expression.Constant(this), nameof(CurrentRequestContext));
        var currentTenantId = Expression.Property(currentContext, nameof(RequestContext.TenantId));
        var tenantBoundary = Expression.Equal(tenantId, currentTenantId);
        var notDeleted = Expression.Equal(isDeleted, Expression.Constant(false));
        return Expression.Lambda(Expression.AndAlso(tenantBoundary, notDeleted), parameter);
    }

    private RequestContext CurrentRequestContext => requestContextAccessor.Current;

    private void AddOutboxMessages()
    {
        var aggregateRoots = ChangeTracker.Entries<AggregateRoot>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity)
            .ToArray();

        foreach (var aggregateRoot in aggregateRoots)
        {
            foreach (var domainEvent in aggregateRoot.DomainEvents)
            {
                OutboxMessages.Add(ToOutboxMessage(domainEvent));
            }

            aggregateRoot.ClearDomainEvents();
        }
    }

    private static OutboxMessage ToOutboxMessage(IDomainEvent domainEvent)
    {
        var envelope = new EventEnvelope(
            domainEvent.EventId,
            domainEvent.TenantId,
            domainEvent.CorrelationId,
            ToEventType(domainEvent),
            domainEvent.Version,
            domainEvent.OccurredAt,
            JsonSerializer.Serialize(domainEvent, domainEvent.GetType()));

        return new OutboxMessage
        {
            Id = envelope.EventId,
            TenantId = envelope.TenantId,
            CorrelationId = envelope.CorrelationId,
            EventType = envelope.EventType,
            Payload = envelope.Payload,
            Version = envelope.Version,
            CreatedAt = envelope.OccurredAt
        };
    }

    internal static string ToEventType(IDomainEvent domainEvent) => domainEvent.GetType().Name switch
    {
        "UserRegisteredDomainEvent" => "identity.user-registered.v1",
        "UserLoggedInDomainEvent" => "identity.user-logged-in.v1",
        "UserDisabledDomainEvent" => "identity.user-disabled.v1",
        "MfaChallengedDomainEvent" => "identity.mfa-challenged.v1",
        "MfaEnabledDomainEvent" => "identity.mfa-enabled.v1",
        "MfaVerifiedDomainEvent" => "identity.mfa-verified.v1",
        "SessionRevokedDomainEvent" => "identity.session-revoked.v1",
        _ => $"identity.{domainEvent.GetType().Name.ToLowerInvariant()}.v1"
    };
}
