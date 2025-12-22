using ACommerce.Versions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Versions.Configurations;

public class AppVersionConfiguration : IEntityTypeConfiguration<AppVersion>
{
    public void Configure(EntityTypeBuilder<AppVersion> builder)
    {
        builder.ToTable("AppVersions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApplicationCode)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ApplicationNameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ApplicationNameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.VersionNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ReleaseNotesAr)
            .HasMaxLength(4000);

        builder.Property(x => x.ReleaseNotesEn)
            .HasMaxLength(4000);

        builder.Property(x => x.UpdateUrl)
            .HasMaxLength(500);

        builder.Property(x => x.DownloadUrl)
            .HasMaxLength(500);

        builder.Property(x => x.MinimumSupportedVersion)
            .HasMaxLength(50);

        builder.Property(x => x.Metadata)
            .HasColumnType("text");

        // Index for fast lookup by application and version
        builder.HasIndex(x => new { x.ApplicationCode, x.VersionNumber })
            .IsUnique()
            .HasDatabaseName("IX_AppVersions_ApplicationCode_VersionNumber");

        // Index for status queries
        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_AppVersions_Status");

        // Index for active versions
        builder.HasIndex(x => new { x.ApplicationCode, x.Status, x.IsActive })
            .HasDatabaseName("IX_AppVersions_ApplicationCode_Status_IsActive");

        // Index for release date ordering
        builder.HasIndex(x => x.ReleaseDate)
            .HasDatabaseName("IX_AppVersions_ReleaseDate");

        // Soft delete filter
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
