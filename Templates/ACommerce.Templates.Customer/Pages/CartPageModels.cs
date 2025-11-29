namespace ACommerce.Templates.Customer.Pages;

public class CartItemModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Variant { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
    public decimal Price { get; set; }
    public decimal OldPrice { get; set; }
    public int Quantity { get; set; }
    public int MaxQuantity { get; set; } = 99;
}

public class CouponModel
{
    public string Code { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercent { get; set; }
    public string DiscountText => DiscountPercent > 0
        ? $"{DiscountPercent}%"
        : $"{DiscountAmount:N2}";
}
