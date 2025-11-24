using ACommerce.Orders.Enums;
using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Orders.Entities;

/// <summary>
/// الطلب - مبني على Transactions (وثيقة)
/// </summary>
public class Order : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// رقم الطلب
	/// </summary>
	public required string OrderNumber { get; set; }

	/// <summary>
	/// معرف العميل
	/// </summary>
	public required string CustomerId { get; set; }

	/// <summary>
	/// معرف البائع (في حالة طلب واحد لبائع واحد)
	/// أو null إذا كان الطلب يحتوي على منتجات من بائعين متعددين
	/// </summary>
	public Guid? VendorId { get; set; }

	/// <summary>
	/// حالة الطلب
	/// </summary>
	public OrderStatus Status { get; set; } = OrderStatus.Pending;

	/// <summary>
	/// المجموع الفرعي (قبل الضرائب والشحن)
	/// </summary>
	public decimal Subtotal { get; set; }

	/// <summary>
	/// مبلغ الخصم
	/// </summary>
	public decimal DiscountAmount { get; set; }

	/// <summary>
	/// كود الخصم
	/// </summary>
	public string? CouponCode { get; set; }

	/// <summary>
	/// الضريبة
	/// </summary>
	public decimal TaxAmount { get; set; }

	/// <summary>
	/// تكلفة الشحن
	/// </summary>
	public decimal ShippingCost { get; set; }

	/// <summary>
	/// المجموع الكلي
	/// </summary>
	public decimal Total { get; set; }

	/// <summary>
	/// العملة
	/// </summary>
	public required string Currency { get; set; }

	/// <summary>
	/// عنوان الشحن
	/// </summary>
	public string? ShippingAddress { get; set; }

	/// <summary>
	/// عنوان الفواتير
	/// </summary>
	public string? BillingAddress { get; set; }

	/// <summary>
	/// معرف الدفع
	/// </summary>
	public string? PaymentId { get; set; }

	/// <summary>
	/// طريقة الدفع
	/// </summary>
	public string? PaymentMethod { get; set; }

	/// <summary>
	/// معرف الشحنة
	/// </summary>
	public string? ShipmentId { get; set; }

	/// <summary>
	/// رقم التتبع
	/// </summary>
	public string? TrackingNumber { get; set; }

	/// <summary>
	/// ملاحظات العميل
	/// </summary>
	public string? CustomerNotes { get; set; }

	/// <summary>
	/// ملاحظات داخلية
	/// </summary>
	public string? InternalNotes { get; set; }

	/// <summary>
	/// بنود الطلب
	/// </summary>
	public List<OrderItem> Items { get; set; } = new();

	/// <summary>
	/// تاريخ التأكيد
	/// </summary>
	public DateTime? ConfirmedAt { get; set; }

	/// <summary>
	/// تاريخ الشحن
	/// </summary>
	public DateTime? ShippedAt { get; set; }

	/// <summary>
	/// تاريخ التسليم
	/// </summary>
	public DateTime? DeliveredAt { get; set; }

	/// <summary>
	/// تاريخ الإلغاء
	/// </summary>
	public DateTime? CancelledAt { get; set; }

	/// <summary>
	/// بيانات إضافية
	/// </summary>
	public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// بند في الطلب
/// </summary>
public class OrderItem : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// معرف الطلب
	/// </summary>
	public Guid OrderId { get; set; }

	/// <summary>
	/// معرف العرض (Listing)
	/// </summary>
	public Guid ListingId { get; set; }

	/// <summary>
	/// معرف البائع
	/// </summary>
	public Guid VendorId { get; set; }

	/// <summary>
	/// معرف المنتج
	/// </summary>
	public Guid ProductId { get; set; }

	/// <summary>
	/// اسم المنتج (نسخة)
	/// </summary>
	public required string ProductName { get; set; }

	/// <summary>
	/// SKU
	/// </summary>
	public string? Sku { get; set; }

	/// <summary>
	/// الكمية
	/// </summary>
	public int Quantity { get; set; }

	/// <summary>
	/// السعر الوحدوي
	/// </summary>
	public decimal UnitPrice { get; set; }

	/// <summary>
	/// المجموع
	/// </summary>
	public decimal Total => UnitPrice * Quantity;

	/// <summary>
	/// الخصم على البند
	/// </summary>
	public decimal? ItemDiscount { get; set; }

	/// <summary>
	/// العمولة (للمنصة)
	/// </summary>
	public decimal CommissionAmount { get; set; }

	/// <summary>
	/// المبلغ الصافي للبائع
	/// </summary>
	public decimal VendorAmount { get; set; }

	/// <summary>
	/// بيانات إضافية
	/// </summary>
	public Dictionary<string, string> Metadata { get; set; } = new();
}
