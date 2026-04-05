using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.RealEstate.Api.Entities;

/// <summary>
/// كيان العقار - يرث IBaseEntity من SharedKernel.
/// مصمم التطبيق يحدد الأعمدة التي تناسب منصته.
/// هذه أعمدة تشبه ما يعرضه عشير.
/// </summary>
public class Property : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    // === البيانات الأساسية ===
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public string Category { get; set; } = default!;        // residential, commercial, land
    public string PropertyType { get; set; } = default!;     // apartment, villa, office, shop
    public string Purpose { get; set; } = default!;          // sale, rent

    // === الموقع ===
    public string City { get; set; } = default!;
    public string District { get; set; } = default!;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // === المواصفات ===
    public decimal Price { get; set; }
    public string Currency { get; set; } = "SAR";
    public decimal Area { get; set; }                        // بالمتر المربع
    public int? Rooms { get; set; }
    public int? Bathrooms { get; set; }
    public int? Floor { get; set; }
    public bool? Furnished { get; set; }

    // === المالك ===
    public Guid OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public string? OwnerPhone { get; set; }

    // === الحالة ===
    public string Status { get; set; } = "active";          // draft, active, rented, sold, expired

    // === الصور (JSON) ===
    public string? ImagesJson { get; set; }
    public string? FeaturedImage { get; set; }
}

/// <summary>
/// حجز/حدث اهتمام بعقار
/// </summary>
public class PropertyInquiry : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public Guid PropertyId { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserPhone { get; set; }
    public string? Message { get; set; }
    public string Status { get; set; } = "pending";         // pending, contacted, completed, cancelled
}
