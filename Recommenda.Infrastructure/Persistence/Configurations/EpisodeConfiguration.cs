using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Recommenda.Domain.Entities;
using Recommenda.Infrastructure.Persistence.Converters;

namespace Recommenda.Infrastructure.Persistence.Configurations;

public class EpisodeConfiguration : BaseEntityConfiguration<Episode>
{
    public override void Configure(EntityTypeBuilder<Episode> builder)
    {
        base.Configure(builder);

        builder.ToTable("Episode");

        builder.Property(e => e.Number)
            .IsRequired();

        builder.Property(e => e.Title)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.DurationInMinutes)
            .IsRequired();

        builder.Property(e => e.ReleaseDate)
            .HasConversion<DateOnlyConverter>()
            .HasColumnType("date")
            .IsRequired();

        builder.HasIndex(e => new { e.SeasonId, e.Number })
            .IsUnique();

        builder.HasOne(e => e.Season)
            .WithMany(s => s.Episodes)
            .HasForeignKey(e => e.SeasonId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
