using SharedKernel.Domain.Guards;

namespace SharedKernel.Domain.Primitives;

public abstract class Entity
{
    protected Entity(Guid id, Guid tenantId)
    {
        Id = Guard.NotEmpty(id, nameof(id));
        TenantId = Guard.NotEmpty(tenantId, nameof(tenantId));
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
        Version = 1;
    }

    public Guid Id { get; private init; }
    public Guid TenantId { get; private init; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public int Version { get; private set; }
    public bool IsDeleted { get; private set; }

    protected void MarkUpdated()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
        Version++;
    }

    protected void SoftDelete()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        MarkUpdated();
    }
}
