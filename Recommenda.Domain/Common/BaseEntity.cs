namespace Recommenda.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public bool Active { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdateAt { get; private set; } = DateTime.UtcNow;

    public void Deactivate() { Active = false; MarkUpdated(); }
    public void Activate() { Active = true; MarkUpdated(); }
    public void MarkUpdated() => UpdateAt = DateTime.UtcNow;
}