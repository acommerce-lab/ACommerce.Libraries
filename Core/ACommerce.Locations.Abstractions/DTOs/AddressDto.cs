using ACommerce.Locations.Abstractions.Entities;

namespace ACommerce.Locations.Abstractions.DTOs;

// ════════════════════════════════════════════════════════════
// Address DTOs
// ════════════════════════════════════════════════════════════

public class AddressResponseDto
{
    public Guid Id { get; set; }
    public string? Label { get; set; }
    public AddressType Type { get; set; }
    public string TypeName => Type.ToString();

    // Location IDs
    public Guid CountryId { get; set; }
    public Guid? RegionId { get; set; }
    public Guid? CityId { get; set; }
    public Guid? NeighborhoodId { get; set; }

    // Location Names
    public string? CountryName { get; set; }
    public string? RegionName { get; set; }
    public string? CityName { get; set; }
    public string? NeighborhoodName { get; set; }

    // Address Details
    public string? Street { get; set; }
    public string? BuildingNumber { get; set; }
    public string? UnitNumber { get; set; }
    public string? Floor { get; set; }
    public string? PostalCode { get; set; }
    public string? NationalAddressCode { get; set; }
    public string? AdditionalDetails { get; set; }

    // GPS
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Recipient
    public string? RecipientName { get; set; }
    public string? RecipientPhone { get; set; }

    public bool IsDefault { get; set; }

    /// <summary>
    /// العنوان المنسق للعرض
    /// </summary>
    public string? FormattedAddress { get; set; }
}

public class CreateAddressDto
{
    public required string EntityType { get; set; }
    public required Guid EntityId { get; set; }

    public string? Label { get; set; }
    public AddressType Type { get; set; } = AddressType.Other;

    public required Guid CountryId { get; set; }
    public Guid? RegionId { get; set; }
    public Guid? CityId { get; set; }
    public Guid? NeighborhoodId { get; set; }

    public string? Street { get; set; }
    public string? BuildingNumber { get; set; }
    public string? UnitNumber { get; set; }
    public string? Floor { get; set; }
    public string? PostalCode { get; set; }
    public string? NationalAddressCode { get; set; }
    public string? AdditionalDetails { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public string? RecipientName { get; set; }
    public string? RecipientPhone { get; set; }

    public bool IsDefault { get; set; }
}

public class UpdateAddressDto
{
    public string? Label { get; set; }
    public AddressType? Type { get; set; }

    public Guid? CountryId { get; set; }
    public Guid? RegionId { get; set; }
    public Guid? CityId { get; set; }
    public Guid? NeighborhoodId { get; set; }

    public string? Street { get; set; }
    public string? BuildingNumber { get; set; }
    public string? UnitNumber { get; set; }
    public string? Floor { get; set; }
    public string? PostalCode { get; set; }
    public string? NationalAddressCode { get; set; }
    public string? AdditionalDetails { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public string? RecipientName { get; set; }
    public string? RecipientPhone { get; set; }

    public bool? IsDefault { get; set; }
}
