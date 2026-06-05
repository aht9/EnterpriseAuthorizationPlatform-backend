using System.Linq.Expressions;
using System.Text.Json;
using AuthorizationService.Application.Common.Abstractions;
using AuthorizationService.Domain.Aggregates.Permission;
using AuthorizationService.Domain.Aggregates.PermissionGrant;
using AuthorizationService.Domain.Aggregates.Role;
using AuthorizationService.Domain.Aggregates.RoleAssignment;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Contracts.Events;
using SharedKernel.Domain.Events;
using SharedKernel.Domain.Primitives;
using SharedKernel.Infrastructure.Outbox;

namespace AuthorizationService.Infrastructure.Persistence;

public sealed class AuthorizationDbContext(DbContextOptions<AuthorizationDbContext> options, IRequestContext requestContext) : DbContext(options)
{
    private const string Schema = "Authorization";

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RoleAssignment> RoleAssignments => Set<RoleAssignment>();
    public DbSet<PermissionGrant> PermissionGrants => Set<PermissionGrant>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthorizationDbContext).Assembly);
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(entityType => typeof(Entity).IsAssignableFrom(entityType.ClrType)))
        {
            entityType.SetQueryFilter(CreateTenantSoftDeleteFilter(entityType.ClrType));
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddOutboxMessages();
        return await base.SaveChangesAsync(cancellationToken);
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

    private RequestContext CurrentRequestContext => requestContext.Current;

    private void AddOutboxMessages()
    {
        var aggregateRoots = ChangeTracker.Entries<AggregateRoot>().Where(entry => entry.Entity.DomainEvents.Count > 0).Select(entry => entry.Entity).ToArray();
        foreach (var aggregateRoot in aggregateRoots)
        {
            foreach (var domainEvent in aggregateRoot.DomainEvents) OutboxMessages.Add(ToOutboxMessage(domainEvent));
            aggregateRoot.ClearDomainEvents();
        }
    }

    internal static OutboxMessage ToOutboxMessage(IDomainEvent domainEvent)
    {
        var envelope = new EventEnvelope(domainEvent.EventId, domainEvent.TenantId, domainEvent.CorrelationId, ToEventType(domainEvent), domainEvent.Version, domainEvent.OccurredAt, JsonSerializer.Serialize(domainEvent, domainEvent.GetType()));
        return new OutboxMessage { Id = envelope.EventId, TenantId = envelope.TenantId, CorrelationId = envelope.CorrelationId, EventType = envelope.EventType, Payload = envelope.Payload, Version = envelope.Version, CreatedAt = envelope.OccurredAt };
    }

    internal static string ToEventType(IDomainEvent domainEvent) => domainEvent.GetType().Name switch
    {
        "RoleCreatedDomainEvent" => "authorization.role-created.v1",
        "RoleActivatedDomainEvent" => "authorization.role-activated.v1",
        "RoleDeactivatedDomainEvent" => "authorization.role-deactivated.v1",
        "RoleAssignedDomainEvent" => "authorization.role-assigned.v1",
        "RoleRevokedDomainEvent" => "authorization.role-revoked.v1",
        "PermissionCreatedDomainEvent" => "authorization.permission-created.v1",
        "PermissionGrantedDomainEvent" => "authorization.permission-granted.v1",
        "PermissionRevokedDomainEvent" => "authorization.permission-revoked.v1",
        "AuthorizationEvaluatedDomainEvent" => "authorization.evaluated.v1",
        _ => $"authorization.{domainEvent.GetType().Name.ToLowerInvariant()}.v1"
    };
}
