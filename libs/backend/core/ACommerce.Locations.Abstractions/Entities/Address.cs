using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Locations.Abstractions.Entities;

/// <summary>
/// العنوان الكامل - يمكن ربطه بأي كيان
/// </summary>
public class Address : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>
    /// نوع الكيان المرتبط (User, Vendor, Store, etc)
    /// </summary>
    public required string EntityType { get; set; }

    /// <summary>
    /// معرف الكيان المرتبط
    /// </summary>
    public required Guid EntityId { get; set; }

    /// <summary>
    /// تسمية العنوان (المنزل، العمل، إلخ)
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// نوع العنوان
    /// </summary>
    public AddressType Type { get; set; } = AddressType.Other;

    /// <summary>
    /// معرف الدولة
    /// </summary>
    public Guid CountryId { get; set; }

    /// <summary>
    /// معرف المنطقة
    /// </summary>
    public Guid? RegionId { get; set; }

    /// <summary>
    /// معرف المدينة
    /// </summary>
    public Guid? CityId { get; set; }

    /// <summary>
    /// معرف الحي
    /// </summary>
    public Guid? NeighborhoodId { get; set; }

    /// <summary>
    /// اسم الشارع
    /// </summary>
    public string? Street { get; set; }

    /// <summary>
    /// رقم المبنى
    /// </summary>
    public string? BuildingNumber { get; set; }

    /// <summary>
    /// رقم الوحدة/الشقة
    /// </summary>
    public string? UnitNumber { get; set; }

    /// <summary>
    /// الدور
    /// </summary>
    public string? Floor { get; set; }

    /// <summary>
    /// الرمز البريدي
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// الرمز الوطني للعنوان (مثل العنوان الوطني السعودي)
    /// </summary>
    public string? NationalAddressCode { get; set; }

    /// <summary>
    /// تفاصيل إضافية
    /// </summary>
    public string? AdditionalDetails { get; set; }

    /// <summary>
    /// خط العرض
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// خط الطول
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// العنوان الافتراضي
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// اسم المستلم
    /// </summary>
    public string? RecipientName { get; set; }

    /// <summary>
    /// رقم هاتف المستلم
    /// </summary>
    public string? RecipientPhone { get; set; }

    // Navigation Properties
    public Country? Country { get; set; }
    public Region? Region { get; set; }
    public City? City { get; set; }
    public Neighborhood? Neighborhood { get; set; }

    /// <summary>
    /// العنوان المنسق للعرض
    /// </summary>
    public string GetFormattedAddress()
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(BuildingNumber))
            parts.Add(BuildingNumber);

        if (!string.IsNullOrEmpty(Street))
            parts.Add(Street);

        if (Neighborhood != null)
            parts.Add(Neighborhood.Name);

        if (City != null)
            parts.Add(City.Name);

        if (Region != null)
            parts.Add(Region.Name);

        if (Country != null)
            parts.Add(Country.Name);

        if (!string.IsNullOrEmpty(PostalCode))
            parts.Add(PostalCode);

        return string.Join(", ", parts);
    }
}

/// <summary>
/// نوع العنوان
/// </summary>
public enum AddressType
{
    /// <summary>
    /// المنزل
    /// </summary>
    Home = 1,

    /// <summary>
    /// العمل
    /// </summary>
    Work = 2,

    /// <summary>
    /// الشحن
    /// </summary>
    Shipping = 3,

    /// <summary>
    /// الفوترة
    /// </summary>
    Billing = 4,

    /// <summary>
    /// أخرى
    /// </summary>
    Other = 99
}
