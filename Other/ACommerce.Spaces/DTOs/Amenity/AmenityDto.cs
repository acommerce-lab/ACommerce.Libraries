namespace ACommerce.Spaces.DTOs.Amenity;

public class CreateAmenityDto
{
    public Guid SpaceId { get; set; }
    public required string Name { get; set; }
    public string? NameEn { get; set; }
    public string? Category { get; set; }
    public string? Icon { get; set; }
    public string? Description { get; set; }
    public bool IsFree { get; set; } = true;
    public decimal? ExtraPrice { get; set; }
    public string? PricingUnit { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? Notes { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateAmenityDto
{
    public required string Name { get; set; }
    public string? NameEn { get; set; }
    public string? Category { get; set; }
    public string? Icon { get; set; }
    public string? Description { get; set; }
    public bool IsFree { get; set; }
    public decimal? ExtraPrice { get; set; }
    public string? PricingUnit { get; set; }
    public bool IsAvailable { get; set; }
    public string? Notes { get; set; }
    public int SortOrder { get; set; }
}

public class AmenityResponseDto
{
    public Guid Id { get; set; }
    public Guid SpaceId { get; set; }
    public string Name { get; set; } = default!;
    public string? NameEn { get; set; }
    public string? Category { get; set; }
    public string? Icon { get; set; }
    public string? Description { get; set; }
    public bool IsFree { get; set; }
    public decimal? ExtraPrice { get; set; }
    public string? PricingUnit { get; set; }
    public bool IsAvailable { get; set; }
    public string? Notes { get; set; }
    public int SortOrder { get; set; }
}
