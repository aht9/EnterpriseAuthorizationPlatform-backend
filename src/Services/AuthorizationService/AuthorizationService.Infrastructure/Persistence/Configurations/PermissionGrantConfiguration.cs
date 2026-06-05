using AuthorizationService.Domain.Aggregates.PermissionGrant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthorizationService.Infrastructure.Persistence.Configurations;

public sealed class PermissionGrantConfiguration : IEntityTypeConfiguration<PermissionGrant>
{
    public void Configure(EntityTypeBuilder<PermissionGrant> builder)
    {
        builder.ToTable("authorization_permission_grants");
        builder.HasKey(grant => new { grant.TenantId, grant.Id });
        builder.HasIndex(grant => new { grant.TenantId, grant.RoleId, grant.PermissionId });
        builder.Property(grant => grant.Id).ValueGeneratedNever();
        builder.Property(grant => grant.TenantId).ValueGeneratedNever();
        builder.Property(grant => grant.Version).IsConcurrencyToken();
        builder.Ignore(grant => grant.IsActive);
        builder.Ignore(grant => grant.DomainEvents);
    }
}
