using ACommerce.Notifications.Channels.Firebase.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Notifications.Channels.Firebase.EntityFramework.Configurations;

/// <summary>
/// تكوين جدول DeviceTokens في قاعدة البيانات
/// </summary>
public class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceTokenEntity>
{
    public void Configure(EntityTypeBuilder<DeviceTokenEntity> builder)
    {
        builder.ToTable("DeviceTokens");

        builder.HasKey(x => x.Id);

        // Token is unique - each device has one token
        builder.HasIndex(x => x.Token)
            .IsUnique();

        // Index for fast user lookup
        builder.HasIndex(x => new { x.UserId, x.IsActive });

        // Index for cleanup queries
        builder.HasIndex(x => x.LastUsedAt);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Platform)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.AppVersion)
            .HasMaxLength(50);

        builder.Property(x => x.DeviceModel)
            .HasMaxLength(100);

        builder.Property(x => x.MetadataJson)
            .HasMaxLength(2000);
    }
}
