namespace ACommerce.Templates.Customer.Components;

public class BottomNavItem
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? ActiveIcon { get; set; }
    public string Href { get; set; } = "#";
    public bool IsActive { get; set; }
    public int BadgeCount { get; set; }
}
