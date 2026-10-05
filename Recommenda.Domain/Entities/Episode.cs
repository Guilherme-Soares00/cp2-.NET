using Recommenda.Domain.Common;

namespace Recommenda.Domain.Entities;

public class Episode : BaseEntity
{
    public int Number { get; set; }

    public string Title { get; set; } = string.Empty;

    public int DurationInMinutes { get; set; }

    public DateOnly ReleaseDate { get; set; }

    public Guid SeasonId { get; set; }

    public Season Season { get; set; } = null!;

    public Episode()
    {

    }
}