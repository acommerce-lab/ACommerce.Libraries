using ACommerce.Templates.Customer.Components;

namespace ACommerce.Templates.Customer.Pages;

public class ProductDetailsItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? ShortDescription { get; set; }
    public string? LongDescription { get; set; }
    public List<string> Images { get; set; } = new();
    public decimal Price { get; set; }
    public decimal OldPrice { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public int? StockQuantity { get; set; }
    public string? Sku { get; set; }
    public string? DeliveryInfo { get; set; }
    public string? ReturnPolicy { get; set; }
    public string? Warranty { get; set; }
    public List<DynamicAttribute>? Attributes { get; set; }
    public Dictionary<string, string>? Specifications { get; set; }
}

public class ProductReviewItem
{
    public Guid Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAvatar { get; set; }
    public double Rating { get; set; }
    public string? Title { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<string>? Images { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public int HelpfulCount { get; set; }
}

public class AddToCartEventArgs
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public Dictionary<string, string>? SelectedAttributes { get; set; }
}
