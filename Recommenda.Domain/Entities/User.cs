using Recommenda.Domain.Common;

namespace Recommenda.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserConfiguration? Configuration { get; set; }

    public List<Rating> Ratings { get; set; } = [];

    public User()
    {

    }
}
