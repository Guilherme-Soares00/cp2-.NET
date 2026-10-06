using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Recommenda.Domain.Entities;

namespace Recommenda.Infrastructure.Persistence.Configurations;

public class RatingConfiguration : BaseEntityConfiguration<Rating>
{
    public override void Configure(EntityTypeBuilder<Rating> builder)
    {
        base.Configure(builder);

        builder.ToTable("Rating", table =>
            table.HasCheckConstraint("CK_Rating_Score", "`Score` BETWEEN 1 AND 5"));

        builder.Property(r => r.Score)
            .IsRequired();

        builder.HasIndex(r => new { r.UserId, r.ContentId })
            .IsUnique();

        builder.HasOne(r => r.Content)
            .WithMany(c => c.Ratings)
            .HasForeignKey(r => r.ContentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany(u => u.Ratings)
            .HasForeignKey(r => r.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
