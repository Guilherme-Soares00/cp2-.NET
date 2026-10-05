using Recommenda.Domain.Common;

namespace Recommenda.Domain.Entities;

public abstract class Content : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateOnly ReleaseDate { get; set; }
    public string Company { get; set; } = string.Empty;
    public List<Genre> Genres { get; set; } = [];
    public List<Rating> Ratings { get; set; } = [];
}