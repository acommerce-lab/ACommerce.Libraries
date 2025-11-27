using ACommerce.SharedKernel.Abstractions.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACommerce.Cart.Entities;

/// <summary>
/// سلة التسوق
/// </summary>
public class Cart : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// معرف المستخدم (أو Session للضيوف)
	/// </summary>
	public required string UserIdOrSessionId { get; set; }

	/// <summary>
	/// بنود السلة
	/// </summary>
	public List<CartItem> Items { get; set; } = new();

	/// <summary>
	/// كود الخصم
	/// </summary>
	public string? CouponCode { get; set; }

	/// <summary>
	/// مبلغ الخصم
	/// </summary>
	public decimal? DiscountAmount { get; set; }

	/// <summary>
	/// ملاحظات
	/// </summary>
	public string? Notes { get; set; }

    /// <summary>
    /// بيانات إضافية
    /// </summary>
    [NotMapped] public Dictionary<string, string> Metadata { get; set; } = new();

	/// <summary>
	/// حساب المجموع
	/// </summary>
	public decimal GetTotal()
	{
		var subtotal = Items.Sum(item => item.Price * item.Quantity);
		return subtotal - (DiscountAmount ?? 0);
	}
}

/// <summary>
/// بند في السلة
/// </summary>
public class CartItem : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// معرف السلة
	/// </summary>
	public Guid CartId { get; set; }

	/// <summary>
	/// معرف العرض (Listing)
	/// </summary>
	public Guid ListingId { get; set; }

	/// <summary>
	/// الكمية
	/// </summary>
	public int Quantity { get; set; }

	/// <summary>
	/// السعر عند الإضافة
	/// </summary>
	public decimal Price { get; set; }

    /// <summary>
    /// بيانات إضافية
    /// </summary>
    [NotMapped] public Dictionary<string, string> Metadata { get; set; } = new();
}
