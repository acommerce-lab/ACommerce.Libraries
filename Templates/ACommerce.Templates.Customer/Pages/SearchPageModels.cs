namespace ACommerce.Templates.Customer.Pages;

public class SearchProductItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string? ShortDescription { get; set; }
    public string? CategoryName { get; set; }
    public decimal Price { get; set; }
    public decimal OldPrice { get; set; }
    public int DiscountPercent { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public int? StockQuantity { get; set; }
    public bool IsNew { get; set; }
    public bool IsFeatured { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
}

public class FilterCategoryItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public bool IsSelected { get; set; }
}

public class AttributeFilterGroup
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "checkbox"; // checkbox, color, range
    public List<AttributeFilterValue> Values { get; set; } = new();
}

public class AttributeFilterValue
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public bool IsSelected { get; set; }
}

public class SortOption
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
