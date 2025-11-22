using ACommerce.Authentication.TwoFactor.SessionStore.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Authentication.TwoFactor.SessionStore.EntityFramework.Configurations;

public class TwoFactorSessionConfiguration : IEntityTypeConfiguration<TwoFactorSessionEntity>
{
    public void Configure(EntityTypeBuilder<TwoFactorSessionEntity> builder)
    {
        builder.ToTable("TwoFactorSessions");

        builder.HasKey(e => e.TransactionId);

        builder.Property(e => e.TransactionId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Identifier)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Provider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.VerificationCode)
            .HasMaxLength(10);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.ExpiresAt)
            .IsRequired();

        builder.Property(e => e.MetadataJson)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(e => e.Identifier);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ExpiresAt);
    }
}