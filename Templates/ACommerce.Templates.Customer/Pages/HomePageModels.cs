namespace ACommerce.Templates.Customer.Pages;

public class HomeCategoryItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Image { get; set; }
    public int ProductCount { get; set; }
}

public class BannerItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string? ActionText { get; set; }
}

public class HomeProductItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Image { get; set; }
    public decimal Price { get; set; }
    public decimal OldPrice { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
}
