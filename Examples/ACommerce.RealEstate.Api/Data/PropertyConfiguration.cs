using ACommerce.RealEstate.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.RealEstate.Api.Data;

/// <summary>
/// تهيئة جدول العقارات مع فهارس على الأعمدة الثابتة المعروفة.
/// هذا هو الفرق الجوهري: أعمدة ثابتة = فهارس مباشرة = بحث سريع.
/// </summary>
public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties_RealEstate");
        builder.HasKey(p => p.Id);

        // === الفهارس - هنا القوة الحقيقية ===
        builder.HasIndex(p => p.City);
        builder.HasIndex(p => p.District);
        builder.HasIndex(p => p.Purpose);
        builder.HasIndex(p => p.Category);
        builder.HasIndex(p => p.PropertyType);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.Price);
        builder.HasIndex(p => p.Area);
        builder.HasIndex(p => p.Rooms);
        builder.HasIndex(p => p.OwnerId);
        builder.HasIndex(p => p.IsDeleted);
        builder.HasIndex(p => p.CreatedAt);

        // فهرس مركّب للبحث الأكثر شيوعاً
        builder.HasIndex(p => new { p.City, p.Purpose, p.IsDeleted, p.Status });
        builder.HasIndex(p => new { p.Category, p.City, p.Price });
    }
}

public class PropertyInquiryConfiguration : IEntityTypeConfiguration<PropertyInquiry>
{
    public void Configure(EntityTypeBuilder<PropertyInquiry> builder)
    {
        builder.ToTable("PropertyInquiries_RealEstate");
        builder.HasKey(i => i.Id);
        builder.HasIndex(i => i.PropertyId);
        builder.HasIndex(i => i.UserId);
    }
}
