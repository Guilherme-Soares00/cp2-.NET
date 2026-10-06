using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Recommenda.Domain.Entities;

namespace Recommenda.Infrastructure.Persistence.Configurations;

public class UserSettingsConfiguration : BaseEntityConfiguration<UserConfiguration>
{
    public override void Configure(EntityTypeBuilder<UserConfiguration> builder)
    {
        base.Configure(builder);

        builder.ToTable("UserConfiguration");

        builder.Property(c => c.EnableNotifications)
            .IsRequired();

        builder.Property(c => c.Theme)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(c => c.UserId)
            .IsUnique();

        builder.HasOne(c => c.User)
            .WithOne(u => u.Configuration)
            .HasForeignKey<UserConfiguration>(c => c.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
