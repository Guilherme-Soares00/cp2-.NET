namespace Recommenda.Domain.Entities;

public class Serie : Content
{
    public bool IsEnded { get; set; }

    public List<Season> Seasons { get; set; } = [];

    public Serie()
    {

    }
}