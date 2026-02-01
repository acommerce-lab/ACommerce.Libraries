namespace ACommerce.Templates.Customer.Pages;

public class SavedLocation
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "other"; // home, work, other
    public string Address { get; set; } = string.Empty;
    public string? BuildingNumber { get; set; }
    public string? Floor { get; set; }
    public string? Apartment { get; set; }
    public string? Street { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? Notes { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LocationFormData
{
    public string? Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "other";
    public string Address { get; set; } = string.Empty;
    public string? BuildingNumber { get; set; }
    public string? Floor { get; set; }
    public string? Apartment { get; set; }
    public string? Street { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? Notes { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsDefault { get; set; }
}

public class MapLocation
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
    public string? FormattedAddress { get; set; }
}

public static class LocationTypes
{
    public const string Home = "home";
    public const string Work = "work";
    public const string Other = "other";

    public static string GetLabel(string type) => type switch
    {
        Home => "المنزل",
        Work => "العمل",
        _ => "أخرى"
    };

    public static string GetIcon(string type) => type switch
    {
        Home => "bi-house-door",
        Work => "bi-briefcase",
        _ => "bi-geo-alt"
    };
}
