using ACommerce.Subscriptions.Enums;

namespace Ashare.Shared.Models;

/// <summary>
/// طلب إنشاء دفعة اشتراك
/// </summary>
public class CreateSubscriptionPaymentRequest
{
    public Guid PlanId { get; set; }
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
    public string? CouponCode { get; set; }
    public string? ReturnUrl { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// استجابة إنشاء دفعة اشتراك
/// </summary>
public class CreateSubscriptionPaymentResponse
{
    public bool Success { get; set; }
    public Guid SubscriptionId { get; set; }
    public string? PaymentId { get; set; }
    public string? PaymentUrl { get; set; }
    public bool RequiresPayment { get; set; } = true;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";
    public string? Message { get; set; }
}

/// <summary>
/// طلب التحقق من حالة الدفع
/// </summary>
public class VerifyPaymentRequest
{
    public string PaymentId { get; set; } = string.Empty;
    public string? OrderId { get; set; }
    public string? TransactionId { get; set; }
}

/// <summary>
/// استجابة التحقق من الدفع
/// </summary>
public class VerifyPaymentResponse
{
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? SubscriptionId { get; set; }
    public Guid? BookingId { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public string? Message { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// Booking Payment Models - دفع حجز العقارات
// ═══════════════════════════════════════════════════════════════════

/// <summary>
/// طلب إنشاء دفعة حجز (عربون)
/// </summary>
public class CreateBookingPaymentRequest
{
    public Guid SpaceId { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public string? RentType { get; set; }
    public string? ReturnUrl { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// استجابة إنشاء دفعة حجز
/// </summary>
public class CreateBookingPaymentResponse
{
    public bool Success { get; set; }
    public string? BookingId { get; set; }
    public string? PaymentId { get; set; }
    public string? PaymentUrl { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";
    public string? Message { get; set; }
}
