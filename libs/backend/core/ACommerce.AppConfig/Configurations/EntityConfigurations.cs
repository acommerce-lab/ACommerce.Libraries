using ACommerce.AppConfig.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.AppConfig.Configurations;

public class AppConfigEntryConfiguration : IEntityTypeConfiguration<AppConfigEntry>
{
    public void Configure(EntityTypeBuilder<AppConfigEntry> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Key).HasMaxLength(200).IsRequired();
        b.Property(x => x.Value).IsRequired();
        b.Property(x => x.Description).HasMaxLength(500);
        b.HasIndex(x => new { x.Key, x.IsDeleted }).IsUnique().HasFilter("[IsDeleted] = 0").HasDatabaseName("IX_AppConfigEntry_Key_Active");
        b.HasIndex(x => x.IsDeleted);
    }
}

public class UiStringConfiguration : IEntityTypeConfiguration<UiString>
{
    public void Configure(EntityTypeBuilder<UiString> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Key).HasMaxLength(200).IsRequired();
        b.Property(x => x.Language).HasMaxLength(10).IsRequired();
        b.Property(x => x.Value).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);
        // unique (Key, Language) on active rows
        b.HasIndex(x => new { x.Key, x.Language, x.IsDeleted })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_UiString_Key_Lang_Active");
    }
}

public class ThemeTokenConfiguration : IEntityTypeConfiguration<ThemeToken>
{
    public void Configure(EntityTypeBuilder<ThemeToken> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Key).HasMaxLength(100).IsRequired();
        b.Property(x => x.Value).HasMaxLength(200).IsRequired();
        b.HasIndex(x => new { x.Key, x.Mode, x.IsDeleted })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_ThemeToken_Key_Mode_Active");
    }
}

public class FeatureFlagConfiguration : IEntityTypeConfiguration<FeatureFlag>
{
    public void Configure(EntityTypeBuilder<FeatureFlag> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Key).HasMaxLength(200).IsRequired();
        b.Property(x => x.Platforms).HasMaxLength(100);
        b.Property(x => x.MinAppVersion).HasMaxLength(50);
        b.Property(x => x.MaxAppVersion).HasMaxLength(50);
        b.Property(x => x.Description).HasMaxLength(500);
        b.HasIndex(x => new { x.Key, x.IsDeleted })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_FeatureFlag_Key_Active");
    }
}
