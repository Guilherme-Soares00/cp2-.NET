using Recommenda.Domain.Common;

namespace Recommenda.Domain.Entities;

public class Season : BaseEntity
{
    public int Number { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateOnly? ReleaseDate { get; set; }

    public Guid SerieId { get; set; }

    public Serie Serie { get; set; } = null!;

    public List<Episode> Episodes { get; set; } = [];

    public Season()
    {

    }
}