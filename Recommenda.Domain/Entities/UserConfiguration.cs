using Recommenda.Domain.Common;

namespace Recommenda.Domain.Entities;

public class UserConfiguration : BaseEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public bool EnableNotifications { get; set; }

    public string Theme { get; set; } = string.Empty;

    public UserConfiguration()
    {

    }
}