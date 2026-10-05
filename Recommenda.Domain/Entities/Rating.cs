using Recommenda.Domain.Common;

namespace Recommenda.Domain.Entities;

public class Rating : BaseEntity
{
    public Guid ContentId { get; set; }

    public Content Content { get; set; } = null!;

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    // 1..5
    public int Score { get; set; }

    public Rating()
    {

    }
}