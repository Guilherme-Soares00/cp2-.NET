using Microsoft.EntityFrameworkCore;
using Recommenda.Domain.Entities;

namespace Recommenda.Infrastructure.Persistence;

public class RecommendaContext(DbContextOptions<RecommendaContext> options) : DbContext(options)
{
    public DbSet<Content> Contents => Set<Content>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Serie> Series => Set<Serie>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Episode> Episodes => Set<Episode>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserConfiguration> UserConfigurations => Set<UserConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecommendaContext).Assembly);
    }
}