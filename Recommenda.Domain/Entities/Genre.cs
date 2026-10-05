using Recommenda.Domain.Common;

namespace Recommenda.Domain.Entities;

public class Genre : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public List<Content> Contents { get; set; } = [];

    protected Genre() { }

    public Genre(string name, string? description = null) => UpdateDetails(name, description);

    public void UpdateDetails(string name, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        name = name.Trim();
        description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (name.Length > 100)
            throw new ArgumentException("O nome deve ter no máximo 100 caracteres.", nameof(name));
        if (description?.Length > 500)
            throw new ArgumentException("A descrição deve ter no máximo 500 caracteres.", nameof(description));
        Name = name;
        Description = description;
        MarkUpdated();
    }
}