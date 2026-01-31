namespace Order.Shared.Models;

/// <summary>
/// نموذج العرض
/// </summary>
public class OfferDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? TitleEn { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public string? ImageUrl { get; set; }
    public List<string>? Images { get; set; }
    public Guid? CategoryId { get; set; }
    public int? AvailableQuantity { get; set; }
    public VendorDto? Vendor { get; set; }
}

/// <summary>
/// نموذج المتجر
/// </summary>
public class VendorDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? Distance { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsOpen { get; set; }
    public Dictionary<string, WorkingHoursDto>? WorkingHours { get; set; }
}

/// <summary>
/// مواعيد العمل
/// </summary>
public class WorkingHoursDto
{
    public string? Open { get; set; }
    public string? Close { get; set; }
}

/// <summary>
/// نموذج الفئة
/// </summary>
public class CategoryDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? NameEn { get; set; }
    public string? IconUrl { get; set; }
}

/// <summary>
/// استجابة قائمة العروض
/// </summary>
public class OffersResponse
{
    public List<OfferDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
