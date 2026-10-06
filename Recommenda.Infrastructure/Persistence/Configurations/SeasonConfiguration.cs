using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Recommenda.Domain.Entities;
using Recommenda.Infrastructure.Persistence.Converters;

namespace Recommenda.Infrastructure.Persistence.Configurations;

public class SeasonConfiguration : BaseEntityConfiguration<Season>
{
    public override void Configure(EntityTypeBuilder<Season> builder)
    {
        base.Configure(builder);

        builder.ToTable("Season");

        builder.Property(s => s.Number)
            .IsRequired();

        builder.Property(s => s.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(s => s.ReleaseDate)
            .HasConversion<DateOnlyConverter>()
            .HasColumnType("date")
            .IsRequired(false);

        builder.HasIndex(s => new { s.SerieId, s.Number })
            .IsUnique();

        builder.HasOne(s => s.Serie)
            .WithMany(se => se.Seasons)
            .HasForeignKey(s => s.SerieId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
