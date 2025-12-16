using ACommerce.LegalPages.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.LegalPages.Configurations;

public class LegalPageConfiguration : IEntityTypeConfiguration<LegalPage>
{
    public void Configure(EntityTypeBuilder<LegalPage> builder)
    {
        builder.ToTable("LegalPages");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.TitleAr)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(x => x.TitleEn)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(x => x.Url)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(x => x.Icon)
            .HasMaxLength(50);
            
        builder.HasIndex(x => x.Key)
            .IsUnique();
            
        builder.HasIndex(x => x.SortOrder);
        
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
