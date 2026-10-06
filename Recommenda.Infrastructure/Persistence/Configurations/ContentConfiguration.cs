using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Recommenda.Domain.Entities;
using Recommenda.Infrastructure.Persistence.Converters;

namespace Recommenda.Infrastructure.Persistence.Configurations;

public class ContentConfiguration : BaseEntityConfiguration<Content>
{
    public override void Configure(EntityTypeBuilder<Content> builder)
    {
        base.Configure(builder);

        builder.ToTable("Content");

        builder.Property(c => c.Title)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(c => c.ReleaseDate)
            .HasConversion<DateOnlyConverter>()
            .HasColumnType("date")
            .IsRequired();

        builder.Property(c => c.Company)
            .HasMaxLength(150)
            .IsRequired();

        builder.HasMany(c => c.Genres)
            .WithMany(g => g.Contents)
            .UsingEntity<Dictionary<string, object>>(
                "ContentGenre",
                right => right.HasOne<Genre>().WithMany()
                    .HasForeignKey("GenreId").IsRequired().OnDelete(DeleteBehavior.Cascade),
                left => left.HasOne<Content>().WithMany()
                    .HasForeignKey("ContentId").IsRequired().OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("ContentGenre");
                    join.HasKey("ContentId", "GenreId");
                    join.HasIndex("GenreId");
                });

        builder.Property<string>("ContentType").HasMaxLength(8).IsRequired();

        builder.HasDiscriminator<string>("ContentType")
            .HasValue<Movie>("Movie")
            .HasValue<Serie>("Serie");
    }
}
